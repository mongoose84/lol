using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RiotProxy.External.Domain.Entities;
using RiotProxy.Infrastructure.External.Database.Repositories;
using System.Text.Json;

namespace RiotProxy.Infrastructure.External.Riot.RateLimiter
{
    public class RiotRateLimitedJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private bool _jobRunning;

        // Token buckets are set lower for RIOT rate limits  (20 requests/second, 100 requests/2 minutes)
        private readonly TokenBucket _perSecondBucket = new(15, TimeSpan.FromSeconds(1));
        private readonly TokenBucket _perTwoMinuteBucket = new(80, TimeSpan.FromMinutes(2));

        public RiotRateLimitedJob(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Run until the host shuts down
            while (!stoppingToken.IsCancellationRequested)
            {
                var nextRun = DateTime.UtcNow.AddMinutes(2);

                // Wait exactly until the next 2‑minute tick (or break early if cancelled)
                var delay = nextRun - DateTime.UtcNow;
                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, stoppingToken);

                if (_jobRunning)
                    continue;

                await RunJobAsync(stoppingToken);
            }
        }

        private async Task RunJobAsync(CancellationToken ct)
        {
            _jobRunning = true;
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var riotApiClient = scope.ServiceProvider.GetRequiredService<IRiotApiClient>();
                var gamerRepository = scope.ServiceProvider.GetRequiredService<GamerRepository>();
                var matchRepository = scope.ServiceProvider.GetRequiredService<LolMatchRepository>();
                var participantRepository = scope.ServiceProvider.GetRequiredService<LolMatchParticipantRepository>();
                

                Console.WriteLine("RiotRateLimitedJob started.");
                var gamers = await gamerRepository.GetAllGamersAsync();

                Console.WriteLine($"Found {gamers.Count} gamers.");

                // Example: fetch match history for all gamers
                var matchHistory = await GetMatchHistoryFromRiotApi(gamers, riotApiClient, ct);
                Console.WriteLine($"Fetched match history for {matchHistory.Count} matches.");

                await AddMatchHistoryToDb(matchHistory, matchRepository, ct);
                Console.WriteLine("Match history added to DB.");

                // Example: process unprocessed matches
                var unprocessedMatches = await matchRepository.GetUnprocessedMatchesAsync();
                Console.WriteLine($"Found {unprocessedMatches.Count} unprocessed matches.");

                await AddMatchInfoToDb(unprocessedMatches, riotApiClient, participantRepository, matchRepository, ct);

                Console.WriteLine("Match info added to DB.");
                Console.WriteLine("RiotRateLimitedJob completed.");
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                // Log and handle exceptions as needed
                Console.WriteLine($"Error in RiotRateLimitedJob: {ex.Message}");
            }
            finally
            {
                _jobRunning = false;
            }
        }

        private async Task<IList<LolMatch>> GetMatchHistoryFromRiotApi(IList<Gamer> gamers, IRiotApiClient riotApiClient, CancellationToken ct)
        {
            var allMatchHistory = new List<LolMatch>();

            foreach (var gamer in gamers)
            {
                // Wait for permission from both token buckets
                await _perSecondBucket.WaitAsync(ct);
                await _perTwoMinuteBucket.WaitAsync(ct);

                var matchHistory = await riotApiClient.GetMatchHistoryAsync(gamer.Puuid);
                
                allMatchHistory.AddRange(matchHistory);
            }

            return allMatchHistory;
        }

        private async Task AddMatchHistoryToDb(IList<LolMatch> matchHistory, LolMatchRepository matchRepository, CancellationToken ct)
        {
            foreach (var match in matchHistory)
            {
                try
                {
                    await matchRepository.AddMatchAsync(match);
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    Console.WriteLine($"Error adding match history to DB: {ex.Message}");
                }
            }
        }

        private async Task AddMatchInfoToDb(IList<LolMatch> matches,
                                IRiotApiClient riotApiClient,
                                LolMatchParticipantRepository participantRepository,
                                LolMatchRepository matchRepository,
                                CancellationToken ct)
        {

            foreach (var match in matches)
            {
                try
                {
                    // Wait for permission from both token buckets
                    await _perSecondBucket.WaitAsync(ct);
                    await _perTwoMinuteBucket.WaitAsync(ct);

                    var matchInfo = await riotApiClient.GetMatchInfoAsync(match.MatchId);
                    var participant = MapToParticipantEntity(matchInfo, match.Puuid);
                    await participantRepository.AddParticipantAsync(participant);

                    match.GameMode = GetGameMode(matchInfo);
                    match.InfoFetched = true;
                    await matchRepository.UpdateMatchAsync(match);
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    Console.WriteLine($"Error adding match info to DB: {ex.Message}");
                }
            }
        }

        private string GetGameMode(JsonDocument matchInfo)
        {
            // Extract the game mode from the match info
            if (matchInfo.RootElement.TryGetProperty("info", out JsonElement infoElement) &&
                infoElement.TryGetProperty("gameMode", out JsonElement gameModeElement))
            {
                var gameMode = gameModeElement.GetString();
                if (gameMode != null)
                    return gameMode;
            }
            return string.Empty;
        }

        private LolMatchParticipant MapToParticipantEntity(JsonDocument matchInfo, string puuid)
        {
            if (matchInfo.RootElement.TryGetProperty("info", out JsonElement infoElement) &&
                infoElement.TryGetProperty("participants", out JsonElement participantsElement))
            {
                foreach (var participant in participantsElement.EnumerateArray())
                {
                    if (participant.GetProperty("puuid").GetString() == puuid)
                    {
                        var matchId = matchInfo.RootElement.GetProperty("metadata").GetProperty("matchId").GetString();
                        var puuidValue = participant.GetProperty("puuid").GetString();
                        var championName = participant.GetProperty("championName").GetString();
                        if (matchId == null || puuidValue == null || championName == null)
                        {
                            throw new InvalidOperationException("Required participant fields are null.");
                        }
                        
                        return new LolMatchParticipant
                        {
                            MatchId = matchId,
                            Puuid = puuidValue,
                            ChampionName = championName,
                            Win = participant.GetProperty("win").GetBoolean(),
                            Kills = participant.GetProperty("kills").GetInt32(),
                            Deaths = participant.GetProperty("deaths").GetInt32(),
                            Assists = participant.GetProperty("assists").GetInt32(),
                            // Map other properties as needed
                        };
                    }
                }
            }

            throw new InvalidOperationException("Participant not found.");
        }

        private IList<LolMatchParticipant> MapToParticipantEntity(JsonDocument doc)
        {
            // Map the match info data to a LolMatchParticipant entity
            return new List<LolMatchParticipant>
            {
                new LolMatchParticipant
                {
                    // Set properties accordingly
                }
            };
        }
    }
}