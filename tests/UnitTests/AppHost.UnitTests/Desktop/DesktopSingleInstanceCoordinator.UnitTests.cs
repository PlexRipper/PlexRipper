namespace Reaparr.AppHost.UnitTests;

public class DesktopSingleInstanceCoordinatorUnitTests : BaseUnitTest<DesktopSingleInstanceCoordinator>
{
    private static int _instanceNameCounter;

    [Test]
    public void ShouldReturnSuccess_WhenStartListenerRunsMoreThanOnce()
    {
        // Arrange
        using var sut = CreateSut();

        // Act
        var firstResult = sut.StartListener(_ => Task.FromResult(Result.Ok()), CancellationToken);
        var secondResult = sut.StartListener(_ => Task.FromResult(Result.Ok()), CancellationToken);

        // Assert
        firstResult.IsSuccess.ShouldBeTrue();
        secondResult.IsSuccess.ShouldBeTrue();
    }

    [Test]
    public void ShouldAcquirePrimaryOwnershipOnlyOnce_WhenMutexIsAlreadyOwnedByThisCoordinator()
    {
        // Arrange
        using var sut = CreateSut();

        // Act
        var firstAcquire = sut.TryAcquirePrimaryOwnership();
        var secondAcquire = sut.TryAcquirePrimaryOwnership();

        // Assert
        firstAcquire.ShouldBeTrue();
        secondAcquire.ShouldBeTrue();
    }

    [Test]
    public void ShouldReturnFalse_WhenAnotherCoordinatorOwnsPrimaryMutex()
    {
        // Arrange
        var instanceName = CreateInstanceName();
        using var primary = CreateSut(instanceName);
        using var secondary = CreateSut(instanceName);
        primary.TryAcquirePrimaryOwnership().ShouldBeTrue();

        // Act
        var secondaryAcquire = secondary.TryAcquirePrimaryOwnership();

        // Assert
        secondaryAcquire.ShouldBeFalse();
    }

    [Test]
    [Arguments("/tmp/Reaparr-dev.AppImage", "/opt/reaparr/dev")]
    [Arguments(null, "/opt/reaparr/dev")]
    public void ShouldUseSameDerivedInstanceName_WhenExecutionIdentityMatches(
        string? appImagePath,
        string appBaseDirectory
    )
    {
        // Arrange
        var instanceName = CreateInstanceName();
        using var primary = CreateSut(instanceName, appImagePath: appImagePath, appBaseDirectory: appBaseDirectory);
        using var secondary = CreateSut(instanceName, appImagePath: appImagePath, appBaseDirectory: appBaseDirectory);
        primary.TryAcquirePrimaryOwnership().ShouldBeTrue();

        // Act
        var secondaryAcquire = secondary.TryAcquirePrimaryOwnership();

        // Assert
        secondaryAcquire.ShouldBeFalse();
    }

    [Test]
    public void ShouldUseDifferentDerivedInstanceNames_WhenExecutionIdentityDiffers()
    {
        // Arrange
        var instanceName = CreateInstanceName();
        using var primary = CreateSut(
            instanceName,
            appImagePath: "/tmp/Reaparr-dev.AppImage",
            appBaseDirectory: "/opt/reaparr/dev"
        );
        using var secondary = CreateSut(
            instanceName,
            appImagePath: "/tmp/Reaparr-release.AppImage",
            appBaseDirectory: "/opt/reaparr/release"
        );
        primary.TryAcquirePrimaryOwnership().ShouldBeTrue();

        // Act
        var secondaryAcquire = secondary.TryAcquirePrimaryOwnership();

        // Assert
        secondaryAcquire.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldSignalListener_WhenPrimaryListenerReceivesSignal()
    {
        // Arrange
        using var sut = CreateSut();
        var signalReceived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var listenerResult = sut.StartListener(
            _ =>
            {
                signalReceived.SetResult();
                return Task.FromResult(Result.Ok());
            },
            CancellationToken
        );

        // Act
        var signalResult = await sut.SignalPrimaryInstanceAsync(CancellationToken);
        var signalTask = await Task.WhenAny(
            signalReceived.Task,
            Task.Delay(TimeSpan.FromSeconds(2), CancellationToken)
        );

        // Assert
        signalTask.ShouldBe(signalReceived.Task);
        listenerResult.IsSuccess.ShouldBeTrue();
        signalResult.IsSuccess.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnFailure_WhenNoPrimaryListenerReceivesSignal()
    {
        // Arrange
        using var sut = CreateSut();

        // Act
        var result = await sut.SignalPrimaryInstanceAsync(CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    private DesktopSingleInstanceCoordinator CreateSut(
        string? instanceName = null,
        string? appImagePath = null,
        string? appBaseDirectory = null
    ) =>
        new(
            Log,
            instanceName ?? CreateInstanceName(),
            appImagePath is null ? null : () => appImagePath,
            appBaseDirectory is null ? null : () => appBaseDirectory
        );

    private static string CreateInstanceName() =>
        $"Reaparr.Desktop.SingleInstance.Tests.{System.Environment.ProcessId}.{Interlocked.Increment(ref _instanceNameCounter)}";
}
