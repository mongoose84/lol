using System;
using System.Threading;
using System.Threading.Tasks;

namespace RiotProxy.Infrastructure.External.Riot.RateLimiter
{
    public sealed class TokenBucket
    {
        private readonly int _capacity;
        private readonly TimeSpan _refillPeriod;
        private int _tokens;
        private readonly SemaphoreSlim _semaphore;

        public TokenBucket(int capacity, TimeSpan refillPeriod)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            if (refillPeriod <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(refillPeriod));

            _capacity = capacity;
            _refillPeriod = refillPeriod;
            _tokens = capacity;
            _semaphore = new SemaphoreSlim(capacity, capacity);

            // Periodic refill
            var timer = new Timer(_ => Refill(), null, refillPeriod, refillPeriod);
        }

        private void Refill()
        {
            // Compute how many tokens we can add without exceeding capacity
            int toAdd = _capacity - _tokens;
            if (toAdd <= 0) return;

            // Release that many permits on the semaphore
            _semaphore.Release(toAdd);
            Interlocked.Add(ref _tokens, toAdd);
        }

        public async Task WaitAsync(CancellationToken ct)
        {
            // Acquire a permit; this will block until a token is available
            await _semaphore.WaitAsync(ct).ConfigureAwait(false);
            Interlocked.Decrement(ref _tokens);
        }
    }
}