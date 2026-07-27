using System.Threading.Channels;

namespace EntityFrameworkCore.Sqlite.Concurrency;

internal sealed class SqliteWriteQueue : IAsyncDisposable
{
    private readonly Channel<IWriteRequest> _channel;
    private readonly Task _writerTask;

    internal SqliteWriteQueue(int? capacity)
    {
        _channel = capacity.HasValue
            ? Channel.CreateBounded<IWriteRequest>(new BoundedChannelOptions(capacity.Value)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            })
            : Channel.CreateUnbounded<IWriteRequest>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

        _writerTask = Task.Run(RunAsync);
    }

    internal async Task<T> EnqueueAsync<T>(Func<Task<T>> work, CancellationToken ct)
    {
        // Reentrancy: already inside the writer loop's async context — execute directly
        // to prevent deadlock from re-queuing behind ourselves.
        if (SqliteConnectionEnhancer.IsWriteLockHeld.Value)
            return await work();

        var request = new WriteRequest<T>(work);
        await _channel.Writer.WriteAsync(request, ct);
#if NETSTANDARD2_0
        return await request.Completion.Task;
#else
        return await request.Completion.Task.WaitAsync(ct);
#endif
    }

    private async Task RunAsync()
    {
#if NETSTANDARD2_0
        while (await _channel.Reader.WaitToReadAsync())
        {
            while (_channel.Reader.TryRead(out var req))
            {
                SqliteConnectionEnhancer.IsWriteLockHeld.Value = true;
                try   { await req.ExecuteAsync(); }
                finally { SqliteConnectionEnhancer.IsWriteLockHeld.Value = false; }
            }
        }
#else
        await foreach (var request in _channel.Reader.ReadAllAsync())
        {
            SqliteConnectionEnhancer.IsWriteLockHeld.Value = true;
            try
            {
                await request.ExecuteAsync();
            }
            finally
            {
                SqliteConnectionEnhancer.IsWriteLockHeld.Value = false;
            }
        }
#endif
    }

    public async ValueTask DisposeAsync()
    {
        _channel.Writer.Complete();
        await _writerTask;
    }
}

internal interface IWriteRequest
{
    Task ExecuteAsync();
}

internal sealed class WriteRequest<T> : IWriteRequest
{
    private readonly Func<Task<T>> _work;
    internal readonly TaskCompletionSource<T> Completion =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal WriteRequest(Func<Task<T>> work) => _work = work;

    public async Task ExecuteAsync()
    {
        try
        {
            Completion.SetResult(await _work());
        }
        catch (Exception ex)
        {
            Completion.SetException(ex);
        }
    }
}
