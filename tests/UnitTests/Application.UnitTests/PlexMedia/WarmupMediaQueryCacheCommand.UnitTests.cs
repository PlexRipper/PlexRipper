using System.Reflection;

namespace Reaparr.Application.UnitTests;

public class WarmupMediaQueryCacheCommandHandlerUnitTests : BaseUnitTest<WarmupMediaQueryCacheCommandHandler>
{
    [Test]
    public async Task ShouldBuildMediaQueryCache_WhenWarmupRuns()
    {
        // Arrange
        await SetupDatabase(8237);
        OverrideSyncQuietPeriod(TimeSpan.Zero);

        Mock.Mock<IMediaQueryCache>().SetupProperty(x => x.SuppressInvalidation, true);
        Mock.Mock<IMediaQueryCache>()
            .Setup(x => x.BuildCache(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(new WarmupMediaQueryCacheCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        Mock.Mock<IMediaQueryCache>().Verify();
        Mock.Mock<IMediaQueryCache>()
            .Verify(x => x.GetMediaAsync(It.IsAny<MediaQueryFilter>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldReturnFailure_WhenFinalBuildFails()
    {
        // Arrange
        await SetupDatabase(8237);
        OverrideSyncQuietPeriod(TimeSpan.Zero);

        Mock.Mock<IMediaQueryCache>().SetupProperty(x => x.SuppressInvalidation, true);
        Mock.Mock<IMediaQueryCache>()
            .Setup(x => x.BuildCache(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Build cache failed"))
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(new WarmupMediaQueryCacheCommand(), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
        Mock.Mock<IMediaQueryCache>().Object.SuppressInvalidation.ShouldBeFalse();
        Mock.Mock<IMediaQueryCache>().Verify();
    }

    [Test]
    public async Task ShouldKeepInvalidationSuppressed_UntilFinalBuildCompletes()
    {
        // Arrange
        await SetupDatabase(8237);
        OverrideSyncQuietPeriod(TimeSpan.Zero);

        Mock.Mock<IMediaQueryCache>().SetupProperty(x => x.SuppressInvalidation, true);
        Mock.Mock<IMediaQueryCache>()
            .Setup(x => x.BuildCache(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                Mock.Mock<IMediaQueryCache>().Object.SuppressInvalidation.ShouldBeTrue();
                return Task.FromResult(Result.Ok());
            })
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(new WarmupMediaQueryCacheCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        var mock = Mock.Mock<IMediaQueryCache>();
        mock.Object.SuppressInvalidation.ShouldBeFalse();
        mock.VerifySet(x => x.SuppressInvalidation = false, Times.Once);
        mock.Verify();
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
