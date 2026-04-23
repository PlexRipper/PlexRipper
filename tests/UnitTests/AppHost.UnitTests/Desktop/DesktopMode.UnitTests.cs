using System.Text.Json;
using Autofac;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Reaparr.Environment;

namespace Reaparr.AppHost.UnitTests;

public class DesktopModeUnitTests : BaseUnitTest<DesktopMode>
{
    [Test]
    public async Task ShouldReturnFailure_WhenServerAddressIsMissingInProduction()
    {
        // Arrange
        using var _ = WithEnvironmentVariablesAsync(
            new Dictionary<string, string?>
            {
                [EnvKeys.ReaparrPlatform] = "desktop",
                [EnvKeys.DotNetEnvironment] = "Production",
            }
        );

        var windowFactory = new FakeDesktopWindowFactory(new FakeDesktopWindow());
        var server = CreateServer(null);
        var sut = CreateSut(server, windowFactory.Create);

        // Act
        var result = await sut.StartAsync(CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(x =>
            x.Message.Contains("could not determine the server address", StringComparison.OrdinalIgnoreCase)
        );
        windowFactory.CreateCalls.ShouldBe(0);
    }

    [Test]
    public async Task ShouldSkipWindowCreation_WhenIntegrationTestModeIsEnabled()
    {
        // Arrange
        using var _ = WithEnvironmentVariablesAsync(
            new Dictionary<string, string?>
            {
                [EnvKeys.ReaparrPlatform] = "desktop",
                [EnvKeys.IntegrationTestMode] = "true",
            }
        );

        var windowFactory = new FakeDesktopWindowFactory(new FakeDesktopWindow());
        var server = CreateServer("http://localhost:5000");
        var sut = CreateSut(server, windowFactory.Create);

        // Act
        var startResult = await sut.StartAsync(CancellationToken);
        var showResult = await sut.ShowMainWindowAsync(CancellationToken);

        // Assert
        startResult.IsSuccess.ShouldBeTrue();
        showResult.IsSuccess.ShouldBeTrue();
        windowFactory.CreateCalls.ShouldBe(0);
    }

    [Test]
    public async Task ShouldKeepDesktopRuntimeAlive_WhenMainWindowIsClosed()
    {
        // Arrange
        using var _ = WithEnvironmentVariablesAsync(
            new Dictionary<string, string?>
            {
                [EnvKeys.ReaparrPlatform] = "desktop",
                [EnvKeys.DotNetEnvironment] = "Production",
            }
        );

        var window = new FakeDesktopWindow();
        var windowFactory = new FakeDesktopWindowFactory(window);
        var server = CreateServer("http://localhost:5000");
        var sut = CreateSut(server, windowFactory.Create);

        await sut.StartAsync(CancellationToken);
        var waitForExitTask = sut.WaitForExitAsync(CancellationToken);

        // Act
        var result = await sut.CloseMainWindowAsync(CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        waitForExitTask.IsCompleted.ShouldBeFalse();
        window.IsClosedToBackground.ShouldBeTrue();
        window.NativeClosePrevented.ShouldBeFalse();
        window.IsDisposed.ShouldBeFalse();

        await sut.ExitAsync(CancellationToken);
        await waitForExitTask;
    }

    [Test]
    public async Task ShouldStartWindowMessageLoop_WhenWaitingForExit()
    {
        // Arrange
        using var _ = WithEnvironmentVariablesAsync(
            new Dictionary<string, string?>
            {
                [EnvKeys.ReaparrPlatform] = "desktop",
                [EnvKeys.DotNetEnvironment] = "Production",
            }
        );

        var window = new FakeDesktopWindow();
        var windowFactory = new FakeDesktopWindowFactory(window);
        var server = CreateServer("http://localhost:5000");
        var sut = CreateSut(server, windowFactory.Create);

        // Act
        var startResult = await sut.StartAsync(CancellationToken);
        window.WaitForCloseCalls.ShouldBe(0);
        var waitForExitTask = sut.WaitForExitAsync(CancellationToken);
        await window.MessageLoopStarted.Task.WaitAsync(CancellationToken);
        var exitResult = await sut.ExitAsync(CancellationToken);
        await waitForExitTask;

        // Assert
        startResult.IsSuccess.ShouldBeTrue();
        exitResult.IsSuccess.ShouldBeTrue();
        window.WaitForCloseCalls.ShouldBe(1);
    }

    [Test]
    public async Task ShouldAllowNativeClose_WhenExitClosesNativeWindow()
    {
        // Arrange
        using var _ = WithEnvironmentVariablesAsync(
            new Dictionary<string, string?>
            {
                [EnvKeys.ReaparrPlatform] = "desktop",
                [EnvKeys.DotNetEnvironment] = "Production",
            }
        );

        var window = new FakeDesktopWindow();
        var windowFactory = new FakeDesktopWindowFactory(window);
        var server = CreateServer("http://localhost:5000");
        var sut = CreateSut(server, windowFactory.Create);

        await sut.StartAsync(CancellationToken);

        // Act
        var result = await sut.ExitAsync(CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        window.NativeClosePrevented.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldRegisterExternalLinkHandler_WhenMainWindowStarts()
    {
        // Arrange
        using var _ = WithEnvironmentVariablesAsync(
            new Dictionary<string, string?>
            {
                [EnvKeys.ReaparrPlatform] = "desktop",
                [EnvKeys.DotNetEnvironment] = "Production",
            }
        );

        var window = new FakeDesktopWindow();
        var windowFactory = new FakeDesktopWindowFactory(window);
        var server = CreateServer("http://localhost:5000");
        var sut = CreateSut(server, windowFactory.Create);

        // Act
        var result = await sut.StartAsync(CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        window.ExternalLinkHandler.ShouldNotBeNull();
    }

    [Test]
    public async Task ShouldOpenExternalBrowser_WhenExternalLinkMessageIsReceived()
    {
        // Arrange
        using var _ = WithEnvironmentVariablesAsync(
            new Dictionary<string, string?>
            {
                [EnvKeys.ReaparrPlatform] = "desktop",
                [EnvKeys.DotNetEnvironment] = "Production",
            }
        );

        var window = new FakeDesktopWindow();
        var windowFactory = new FakeDesktopWindowFactory(window);
        var server = CreateServer("http://localhost:5000");
        var sut = CreateSut(server, windowFactory.Create);

        await sut.StartAsync(CancellationToken);

        // Act
        window.ExternalLinkHandler!.Invoke(
            new DesktopMessageDTO
            {
                Type = DesktopMessageType.ExternalLink,
                Value = "https://github.com/Reaparr/Reaparr",
            }
        );

        // Assert
        window.OpenedExternalUrls.ShouldBe([new Uri("https://github.com/Reaparr/Reaparr")]);
    }

    [Test]
    public async Task ShouldCompleteDesktopRuntime_WhenExitCommandRuns()
    {
        // Arrange
        using var _ = WithEnvironmentVariablesAsync(
            new Dictionary<string, string?>
            {
                [EnvKeys.ReaparrPlatform] = "desktop",
                [EnvKeys.DotNetEnvironment] = "Production",
            }
        );

        var windowFactory = new FakeDesktopWindowFactory(new FakeDesktopWindow());
        var server = CreateServer("http://localhost:5000");
        var sut = CreateSut(server, windowFactory.Create);

        await sut.StartAsync(CancellationToken);
        var waitForExitTask = sut.WaitForExitAsync(CancellationToken);

        // Act
        var result = await sut.ExitAsync(CancellationToken);
        await waitForExitTask;

        // Assert
        result.IsSuccess.ShouldBeTrue();
        waitForExitTask.IsCompleted.ShouldBeTrue();
    }

    private DesktopMode CreateSut(IServer server, Func<Uri, IDesktopWindow> windowFactory) =>
        Mock.Create<DesktopMode>(
            new TypedParameter(typeof(ILogger), Log),
            new TypedParameter(typeof(IServer), server),
            new TypedParameter(typeof(Func<Uri, IDesktopWindow>), windowFactory)
        );

    private static IServer CreateServer(string? address)
    {
        var featureCollection = new FeatureCollection();
        var serverAddresses = new ServerAddressesFeature();
        if (!string.IsNullOrWhiteSpace(address))
            serverAddresses.Addresses.Add(address);

        featureCollection.Set<IServerAddressesFeature>(serverAddresses);

        var server = new Mock<IServer>(MockBehavior.Strict);
        server.SetupGet(x => x.Features).Returns(featureCollection);
        return server.Object;
    }

    private async Task WaitForWindowToCloseToBackground(FakeDesktopWindow window)
    {
        await window.CloseToBackgroundCompletion.Task.WaitAsync(CancellationToken);
    }

    private sealed class FakeDesktopWindowFactory
    {
        private readonly FakeDesktopWindow _window;

        public FakeDesktopWindowFactory(FakeDesktopWindow window)
        {
            _window = window;
        }

        public int CreateCalls { get; private set; }

        public IDesktopWindow Create(Uri uri)
        {
            CreateCalls++;
            _window.LoadedUri = uri;
            return _window;
        }
    }

    private sealed class FakeDesktopWindow : IDesktopWindow
    {
        public Uri? LoadedUri { get; set; }
        public bool IsClosedToBackground { get; private set; }
        public bool IsRestored { get; private set; }
        public bool NativeClosePrevented { get; private set; }
        public bool IsDisposed { get; private set; }
        public int WaitForCloseCalls { get; private set; }
        public bool IsInitialized { get; private set; }
        public List<Uri> OpenedExternalUrls { get; } = [];

        public TaskCompletionSource CloseToBackgroundCompletion { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource MessageLoopStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Func<object?, EventArgs, bool>? WindowClosingHandler { get; private set; }
        public Action<DesktopMessageDTO>? ExternalLinkHandler { get; private set; }

        public void ConfigureWindow() { }

        public void RegisterWindowClosingHandler(Func<object?, EventArgs, bool> handler)
        {
            WindowClosingHandler = handler;
        }

        public void RegisterDesktopMessageHandler(Action<DesktopMessageDTO> handler)
        {
            ExternalLinkHandler = handler;
        }

        public void OpenExternalBrowser(Uri uri)
        {
            OpenedExternalUrls.Add(uri);
        }

        public void CloseToBackground()
        {
            IsClosedToBackground = true;
            CloseToBackgroundCompletion.SetResult();
        }

        public void RestoreFromBackground()
        {
            IsRestored = true;
        }

        public void CloseNativeWindow()
        {
            NativeClosePrevented = WindowClosingHandler?.Invoke(this, EventArgs.Empty) ?? false;
            IsDisposed = true;
            IsInitialized = false;
        }

        public void DisposeWindow()
        {
            IsDisposed = true;
            IsInitialized = false;
        }

        public void WaitForClose()
        {
            WaitForCloseCalls++;
            IsInitialized = true;
            MessageLoopStarted.SetResult();
        }
    }
}
