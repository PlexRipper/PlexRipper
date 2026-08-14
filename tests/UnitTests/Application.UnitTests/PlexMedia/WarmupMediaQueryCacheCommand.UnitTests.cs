using System.Reflection;

namespace Reaparr.Application.UnitTests;

public class WarmupMediaQueryCacheCommandHandlerUnitTests : BaseUnitTest<WarmupMediaQueryCacheCommandHandler>
{
    [Test]
    public async Task ShouldBuildMediaQueryCache_WhenWarmupRuns()
    {
        // Arrange
        await SetupDatabase(8237);

        Mock.Mock<IMediaQueryCache>().SetupProperty(x => x.SuppressInvalidation);
        Mock.Mock<IMediaQueryCache>()
            .Setup(x => x.BuildCache(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        OverrideSyncQuietPeriod(TimeSpan.Zero);

        // Act
        var result = await Sut.ExecuteAsync(new WarmupMediaQueryCacheCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        Mock.Mock<IMediaQueryCache>()
            .Invocations.Count(x => x.Method.Name == nameof(IMediaQueryCache.BuildCache))
            .ShouldBe(2);
        Mock.Mock<IMediaQueryCache>()
            .Verify(x => x.GetMediaAsync(It.IsAny<MediaQueryFilter>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldReturnFailure_WhenFirstBuildFails()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Build cache failed");
        await SetupDatabase(8237);

        Mock.Mock<IMediaQueryCache>().SetupProperty(x => x.SuppressInvalidation);
        Mock.Mock<IMediaQueryCache>()
            .Setup(x => x.BuildCache(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        OverrideSyncQuietPeriod(TimeSpan.Zero);

        // Act
        var result = await Sut.ExecuteAsync(new WarmupMediaQueryCacheCommand(), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
        Mock.Mock<IMediaQueryCache>()
            .Invocations.Count(x => x.Method.Name == nameof(IMediaQueryCache.BuildCache))
            .ShouldBe(1);
    }

    [Test]
    public async Task ShouldSuppressInvalidationDuringSyncStorm_ThenUnsuppressAndRebuild()
    {
        // Arrange
        await SetupDatabase(8237);

        Mock.Mock<IMediaQueryCache>().SetupProperty(x => x.SuppressInvalidation);
        Mock.Mock<IMediaQueryCache>()
            .Setup(x => x.BuildCache(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        OverrideSyncQuietPeriod(TimeSpan.Zero);

        // Act
        var result = await Sut.ExecuteAsync(new WarmupMediaQueryCacheCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var mock = Mock.Mock<IMediaQueryCache>();
        mock.VerifySet(x => x.SuppressInvalidation = true, Times.Once);
        mock.VerifySet(x => x.SuppressInvalidation = false, Times.Once);
    }

    /// <summary>
    /// Overrides the quiet period to avoid real delays in unit tests.
    /// </summary>
    private void OverrideSyncQuietPeriod(TimeSpan period)
    {
        typeof(WarmupMediaQueryCacheCommandHandler)
            .GetProperty("SyncQuietPeriod", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(Sut, period);
    }
}
