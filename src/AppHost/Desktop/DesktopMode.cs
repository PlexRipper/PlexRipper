using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace Reaparr.AppHost;

/// <summary>Manages the Photino desktop window lifecycle.</summary>
public class DesktopMode : IDesktopMode
{
    private readonly Serilog.ILogger _log;
    private readonly IServer _server;
    private readonly Func<Uri, IDesktopWindow> _windowFactory;
    private readonly TaskCompletionSource _exitCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private IDesktopWindow? _window;
    private bool _isExiting;

    /// <summary>Initializes a new instance of <see cref="DesktopMode"/>.</summary>
    public DesktopMode(Serilog.ILogger log, IServer server, Func<Uri, IDesktopWindow> windowFactory)
    {
        _log = log.ForContext<DesktopMode>();
        _server = server;
        _windowFactory = windowFactory;
    }

    /// <inheritdoc />
    public async Task<Result> StartAsync(CancellationToken cancellationToken)
    {
        if (EnvironmentExtensions.IsIntegrationTestMode())
        {
            _log.Here()
                .Warning(
                    "DesktopMode startup skipped in integration test mode to avoid launching the embedded window."
                );
            return Result.Ok();
        }

        _log.Here().Information("Starting DesktopMode");
        return await ShowMainWindowAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result> ShowMainWindowAsync(CancellationToken cancellationToken)
    {
        if (EnvironmentExtensions.IsIntegrationTestMode())
            return Task.FromResult(Result.Ok());

        if (_window is not null)
        {
            _window.RestoreFromBackground();
            return Task.FromResult(Result.Ok());
        }

        var uriResult = GetReaparrUri();
        if (uriResult.IsFailed)
            return Task.FromResult(uriResult.ToResult());

        _window = _windowFactory(uriResult.Value);
        _window.ConfigureWindow();
        _window.RegisterWindowClosingHandler(OnWindowClosing);

        return Task.FromResult(Result.Ok());
    }

    /// <inheritdoc />
    public Task<Result> CloseMainWindowAsync(CancellationToken cancellationToken)
    {
        _window?.CloseToBackground();
        return Task.FromResult(Result.Ok());
    }

    /// <inheritdoc />
    public Task<Result> ExitAsync(CancellationToken cancellationToken)
    {
        _isExiting = true;
        _window?.CloseNativeWindow();
        _window?.DisposeWindow();
        _window = null;
        _exitCompletion.TrySetResult();
        return Task.FromResult(Result.Ok());
    }

    /// <inheritdoc />
    public async Task WaitForExitAsync(CancellationToken cancellationToken)
    {
        if (EnvironmentExtensions.IsIntegrationTestMode())
        {
            await _exitCompletion.Task.WaitAsync(cancellationToken);
            return;
        }

        _window?.WaitForClose();
        await _exitCompletion.Task.WaitAsync(cancellationToken);
    }

    private bool OnWindowClosing(object? sender, EventArgs args)
    {
        if (_isExiting)
            return false;

        _ = CloseMainWindowFromWindowClosingAsync();
        return true;
    }

    private async Task CloseMainWindowFromWindowClosingAsync()
    {
        _log.Here().Debug("Closing the Reaparr desktop window to the background");
        var result = await CloseMainWindowAsync(CancellationToken.None);
        if (result.IsFailed)
            result.LogError();
    }

    private Result<Uri> GetReaparrUri()
    {
        if (EnvironmentExtensions.IsDevelopmentEnvironment())
            return Result.Ok(new Uri("http://localhost:3000"));

        var serverAddressesFeature = _server.Features.Get<IServerAddressesFeature>();
        var serverAddress = serverAddressesFeature?.Addresses.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(serverAddress))
            return Result.Fail("Desktop mode could not determine the server address for the embedded window.");

        var uri = new Uri(serverAddress);
        return Result.Ok(NormalizeWildcardHost(uri));
    }

    private static Uri NormalizeWildcardHost(Uri uri)
    {
        if (uri.Host is not ("0.0.0.0" or "::" or "[::]"))
            return uri;

        var builder = new UriBuilder(uri) { Host = "localhost" };
        return builder.Uri;
    }
}
