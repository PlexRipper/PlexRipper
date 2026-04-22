using System.IO.Pipes;
using System.Text;
using Reaparr.FluentResultExtensions;

namespace Reaparr.AppHost;

/// <summary>
/// Named mutex and named pipe implementation of desktop single-instance coordination.
/// </summary>
public sealed class DesktopSingleInstanceCoordinator : IDesktopSingleInstanceCoordinator
{
    private const string DefaultSingleInstanceName = "Reaparr.Desktop.SingleInstance";
    private const string SignalMessage = "show";

    private readonly Serilog.ILogger _log;
    private readonly string _instanceName;
    private readonly SemaphoreSlim _listenerStartLock = new(1, 1);

    private Mutex? _mutex;
    private CancellationTokenSource? _listenerCancellationTokenSource;
    private Task? _listenerTask;
    private bool _ownsMutex;

    /// <summary>Initializes a new instance of <see cref="DesktopSingleInstanceCoordinator"/>.</summary>
    public DesktopSingleInstanceCoordinator(Serilog.ILogger log, string instanceName = DefaultSingleInstanceName)
    {
        _log = log.ForContext<DesktopSingleInstanceCoordinator>();
        _instanceName = instanceName;
    }

    /// <inheritdoc />
    public bool TryAcquirePrimaryOwnership()
    {
        if (_ownsMutex)
            return true;

        if (_mutex is not null)
            return false;

        _mutex = new Mutex(initiallyOwned: true, _instanceName, out var createdNew);
        _ownsMutex = createdNew;
        return createdNew;
    }

    /// <inheritdoc />
    public async Task<Result> SignalPrimaryInstanceAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var client = new NamedPipeClientStream(
                ".",
                _instanceName,
                PipeDirection.Out,
                PipeOptions.Asynchronous
            );
            await client.ConnectAsync(TimeSpan.FromSeconds(2), cancellationToken);
            await client.WriteAsync(Encoding.UTF8.GetBytes(SignalMessage), cancellationToken);
            await client.FlushAsync(cancellationToken);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    /// <inheritdoc />
    public Result StartListener(Func<CancellationToken, Task<Result>> onSignal, CancellationToken cancellationToken)
    {
        _listenerStartLock.Wait(cancellationToken);
        try
        {
            if (_listenerTask is not null)
                return Result.Ok();

            _listenerCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _listenerTask = Task.Run(
                () => ListenAsync(onSignal, _listenerCancellationTokenSource.Token),
                CancellationToken.None
            );
            return Result.Ok();
        }
        finally
        {
            _listenerStartLock.Release();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _listenerCancellationTokenSource?.Cancel();
        _listenerCancellationTokenSource?.Dispose();
        _listenerStartLock.Dispose();

        try
        {
            if (_ownsMutex)
                _mutex?.ReleaseMutex();
        }
        catch (ObjectDisposedException e)
        {
            _log.Here().Debug(e, "The desktop single-instance mutex was already disposed before release");
        }

        _mutex?.Dispose();
    }

    private async Task ListenAsync(Func<CancellationToken, Task<Result>> onSignal, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using var server = new NamedPipeServerStream(
                    _instanceName,
                    PipeDirection.In,
                    maxNumberOfServerInstances: 1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous
                );

                await server.WaitForConnectionAsync(cancellationToken);
                var buffer = new byte[SignalMessage.Length];
                var bytesRead = await server.ReadAsync(buffer, cancellationToken);
                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (message != SignalMessage)
                {
                    _log.Here().Warning("Received unknown desktop relaunch signal: {Message}", message);
                    continue;
                }

                var result = await onSignal(cancellationToken);
                if (result.IsFailed)
                    result.LogError();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception e)
            {
                Result.Fail(new ExceptionalError(e)).LogError();
            }
        }
    }
}
