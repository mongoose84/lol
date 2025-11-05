using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RiotProxy.Infrastructure.External.Riot.RateLimiter
{
    public class RiotRateLimitedJob : BackgroundService
    {
        private readonly IRiotApiClient _clientFactory;
        private bool _jobRunning;

        // Token buckets are set lower for RIOT rate limits  (20 requests/second, 100 requests/2 minutes)
        private readonly TokenBucket _perSecondBucket = new(15, TimeSpan.FromSeconds(1));
        private readonly TokenBucket _perTwoMinuteBucket = new(80, TimeSpan.FromMinutes(2));

        public RiotRateLimitedJob(IRiotApiClient riotApiClient)
        {
            _clientFactory = riotApiClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Run until the host shuts down
            while (!stoppingToken.IsCancellationRequested)
            {
                var nextRun = DateTime.UtcNow.AddMinutes(2);
                if (_jobRunning)
                    continue;

                await RunJobAsync(stoppingToken);

                // Wait exactly until the next 2‑minute tick (or break early if cancelled)
                var delay = nextRun - DateTime.UtcNow;
                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, stoppingToken);
            }
        }

        private async Task RunJobAsync(CancellationToken ct)
        {
            // Example: pull a batch of work items from a queue or DB
            var workItems = GetWorkItems(); // IEnumerable<string> of endpoint URLs, for instance
            _jobRunning = true;
            foreach (var url in workItems)
            {
                // Respect both limits before each request
                await _perSecondBucket.WaitAsync(ct);
                await _perTwoMinuteBucket.WaitAsync(ct);

                // Fire the request
                try
                {
                    var response = await client.GetAsync(url, ct);
                    response.EnsureSuccessStatusCode();
                    // …process response…
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    // Log and continue on errors
                    Console.WriteLine($"Error fetching {url}: {ex.Message}");
                }
            }
            _jobRunning = false;
        }

        // Stub – replace with real source of work
        private static string[] GetWorkItems() =>
            new[]
            {
                "https://api.riotgames.com/.../endpoint1",
                "https://api.riotgames.com/.../endpoint2",
                // …
            };

        private IList<string> Get()
        {
            throw new NotImplementedException();
        }
    }
}