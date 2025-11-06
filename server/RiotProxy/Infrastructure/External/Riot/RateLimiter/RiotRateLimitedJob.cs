using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RiotProxy.External.Domain.Entities;
using RiotProxy.Infrastructure.External.Database.Repositories;

namespace RiotProxy.Infrastructure.External.Riot.RateLimiter
{
    public class RiotRateLimitedJob : BackgroundService
    {
        private readonly IRiotApiClient _riotApiClient;
        private readonly GamerRepository _gamerRepository;
        private bool _jobRunning;

        // Token buckets are set lower for RIOT rate limits  (20 requests/second, 100 requests/2 minutes)
        private readonly TokenBucket _perSecondBucket = new(15, TimeSpan.FromSeconds(1));
        private readonly TokenBucket _perTwoMinuteBucket = new(80, TimeSpan.FromMinutes(2));

        public RiotRateLimitedJob(IRiotApiClient riotApiClient, GamerRepository gamerRepository)
        {
            _riotApiClient = riotApiClient;
            _gamerRepository = gamerRepository;
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
                var gamers = await _gamerRepository.GetAllGamersAsync();

                // Example: fetch match history for all gamers
                var matchHistory = await GetMatchHistoryFromRiotApi(gamers, ct);
                await AddMatchHistoryToDb(matchHistory, ct);

                // Example: process unprocessed matches
                var unprocessedMatches = await GetUnprocessedMatchesFromDb(ct);
                await AddMatchInfoToDb(unprocessedMatches, ct);

                await MarkMatchAsProcessedInDb(unprocessedMatches, ct);
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

        private async Task<IList<LolMatch>> GetMatchHistoryFromRiotApi(IList<Gamer> gamers, CancellationToken ct)
        {
            var allMatchHistory = new List<LolMatch>();

            foreach (var gamer in gamers)
            {
                // Wait for permission from both token buckets
                await _perSecondBucket.WaitAsync(ct);
                await _perTwoMinuteBucket.WaitAsync(ct);

                var matchHistory = await _riotApiClient.GetMatchHistoryAsync(gamer.Puuid);
                
                allMatchHistory.AddRange(matchHistory);
            }

            return allMatchHistory;
        }

        private async Task AddMatchHistoryToDb(IList<LolMatch> matchHistory, CancellationToken ct)
        {
            try
            {
                foreach (var match in matchHistory)
                {
                    await _gamerRepository.AddMatchToHistoryAsync(match);
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Console.WriteLine($"Error adding match history to DB: {ex.Message}");
            }
        }

        private async Task<IList<LolMatch>> GetUnprocessedMatchesFromDb(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        private async Task AddMatchInfoToDb(IList<LolMatch>  match, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        private async Task MarkMatchAsProcessedInDb(IList<LolMatch>  match, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}