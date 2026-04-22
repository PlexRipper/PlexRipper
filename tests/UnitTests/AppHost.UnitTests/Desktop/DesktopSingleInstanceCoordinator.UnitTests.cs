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

    private DesktopSingleInstanceCoordinator CreateSut(string? instanceName = null) =>
        new(Log, instanceName ?? CreateInstanceName());

    private static string CreateInstanceName() =>
        $"Reaparr.Desktop.SingleInstance.Tests.{System.Environment.ProcessId}.{Interlocked.Increment(ref _instanceNameCounter)}";
}
