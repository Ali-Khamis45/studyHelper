namespace AiStudyOS.Infrastructure.Common;

/// <summary>
/// A single shared async mutual-exclusion lock, DI-managed as a singleton so it coordinates
/// correctly across scoped consumers (e.g. CachedAiKernel — one instance per request) without
/// resorting to static mutable state.
/// </summary>
public interface IAsyncLock
{
    Task<IDisposable> AcquireAsync(CancellationToken ct);
}

public sealed class AsyncLock : IAsyncLock
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<IDisposable> AcquireAsync(CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        return new Releaser(_semaphore);
    }

    private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
    {
        private bool _released;

        public void Dispose()
        {
            if (_released) return;
            _released = true;
            semaphore.Release();
        }
    }
}
