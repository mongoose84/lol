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
                var nextRun = DateTime.UtcNow.AddMinutes(10);

                // Wait exactly until the next 1‑minute tick (or break early if cancelled)
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

                await AddMatchInfoToDb(unprocessedMatches, gamers, riotApiClient, participantRepository, matchRepository, gamerRepository, ct);

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
                    // Check before inserting - cleaner than catching exceptions
                    await matchRepository.AddMatchIfNotExistsAsync(match);
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    Console.WriteLine($"Error adding match history to DB: {ex.Message}");
                }
            }
        }

        private async Task AddMatchInfoToDb(IList<LolMatch> matches,
                        IList<Gamer> gamers,
                        IRiotApiClient riotApiClient,
                        LolMatchParticipantRepository participantRepository,
                        LolMatchRepository matchRepository,
                        GamerRepository gamerRepository,
                        CancellationToken ct)
        {
            foreach (var match in matches)
            {
                var gamer = gamers.FirstOrDefault(g => g.Puuid == match.Puuid);
                try
                {
                    // Wait for permission from both token buckets
                    await _perSecondBucket.WaitAsync(ct);
                    await _perTwoMinuteBucket.WaitAsync(ct);

                    var matchInfoJson = await riotApiClient.GetMatchInfoAsync(match.MatchId);
                    var participant = MapToParticipantEntity(matchInfoJson, match);
                    
                    // Check before inserting
                    await participantRepository.AddParticipantIfNotExistsAsync(participant);

                    match.GameMode = GetGameMode(matchInfoJson);
                    match.InfoFetched = true;
                    await matchRepository.UpdateMatchAsync(match);

                    if (gamer != null && (gamer.LastChecked == DateTime.MinValue || gamer.LastChecked < match.GameEndTimestamp))
                    {
                        gamer.LastChecked = match.GameEndTimestamp;
                        await gamerRepository.UpdateGamerAsync(gamer);
                    }
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    Console.WriteLine($"Error adding match info to DB: {ex.Message}");
                }
            }
        }

        private string GetGameMode(JsonDocument matchInfo)
        {
            if (matchInfo.RootElement.TryGetProperty("info", out var infoElement) &&
                infoElement.TryGetProperty("gameMode", out var gameModeElement))
            {
                if (gameModeElement.ValueKind == JsonValueKind.String)
                    return gameModeElement.GetString() ?? string.Empty;

                // fallback if API changes type unexpectedly
                return gameModeElement.ToString();
            }
            return string.Empty;
        }

        private static long? GetEpochMilliseconds(JsonElement obj, string propertyName)
        {
            if (!obj.TryGetProperty(propertyName, out var el))
                return null;

            return el.ValueKind switch
            {
                JsonValueKind.Number => el.GetInt64(),
                JsonValueKind.String => long.TryParse(el.GetString(), out var v) ? v : null,
                _ => null
            };
        }

        private LolMatchParticipant MapToParticipantEntity(JsonDocument matchInfo, LolMatch match)
        {
            if (matchInfo.RootElement.TryGetProperty("info", out var infoElement) &&
                infoElement.TryGetProperty("participants", out var participantsElement))
            {
                // gameEndTimestamp is epoch ms; fall back to gameCreation if needed
                var endMs = GetEpochMilliseconds(infoElement, "gameEndTimestamp")
                            ?? GetEpochMilliseconds(infoElement, "gameCreation")
                            ?? 0L;

                if (endMs > 0)
                    match.GameEndTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(endMs).UtcDateTime;
                else
                    match.GameEndTimestamp = DateTime.MinValue;

                foreach (var participant in participantsElement.EnumerateArray())
                {
                    if (participant.GetProperty("puuid").GetString() == match.Puuid)
                    {
                        var matchId = matchInfo.RootElement.GetProperty("metadata").GetProperty("matchId").GetString();
                        var puuidValue = participant.GetProperty("puuid").GetString();
                        var championName = participant.GetProperty("championName").GetString();
                        if (matchId == null || puuidValue == null || championName == null)
                            throw new InvalidOperationException("Required participant fields are null.");

                        return new LolMatchParticipant
                        {
                            MatchId = matchId,
                            Puuid = puuidValue,
                            ChampionName = championName,
                            Win = participant.GetProperty("win").GetBoolean(),
                            Kills = participant.GetProperty("kills").GetInt32(),
                            Deaths = participant.GetProperty("deaths").GetInt32(),
                            Assists = participant.GetProperty("assists").GetInt32(),
                        };
                    }
                }
            }

            throw new InvalidOperationException("Participant not found.");
        }
    }
}