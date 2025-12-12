using System;
using System.Threading;
using System.Threading.Tasks;

namespace RiotProxy.Infrastructure.External.Riot
{
    public sealed class RiotTokenBucket : IDisposable
    {
        private readonly int _capacity;
        private readonly TimeSpan _refillPeriod;
        private int _tokens;
        private readonly SemaphoreSlim _semaphore;
        private readonly Timer _timer;
        private bool _disposed;

        public RiotTokenBucket(int capacity, TimeSpan refillPeriod)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            if (refillPeriod <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(refillPeriod));

            _capacity = capacity;
            _refillPeriod = refillPeriod;
            _tokens = capacity;
            _semaphore = new SemaphoreSlim(capacity, capacity);

            // Store timer in a field to prevent GC
            _timer = new Timer(_ => Refill(), null, refillPeriod, refillPeriod);
        }

        private void Refill()
        {
            if (_disposed) return;

            // Use a spin loop with atomic compare-exchange to safely add tokens
            int current, newValue, toAdd;
            do
            {
                current = Volatile.Read(ref _tokens);
                toAdd = _capacity - current;
                
                if (toAdd <= 0) return;
                
                newValue = current + toAdd;
            }
            while (Interlocked.CompareExchange(ref _tokens, newValue, current) != current);

            // Only release after successfully updating _tokens
            _semaphore.Release(toAdd);
        }

        public async Task WaitAsync(CancellationToken ct)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RiotTokenBucket));
            
            // Acquire a permit; this will block until a token is available
            await _semaphore.WaitAsync(ct).ConfigureAwait(false);
            Interlocked.Decrement(ref _tokens);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            
            _timer?.Dispose();
            _semaphore?.Dispose();
        }
    }
}