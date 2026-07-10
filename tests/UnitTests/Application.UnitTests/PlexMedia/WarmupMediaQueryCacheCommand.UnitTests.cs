namespace Reaparr.Application.UnitTests;

public class WarmupMediaQueryCacheCommandHandlerUnitTests : BaseUnitTest<WarmupMediaQueryCacheCommandHandler>
{
    [Test]
    public async Task ShouldBuildMediaQueryCache_WhenWarmupRuns()
    {
        // Arrange
        await SetupDatabase(8237);

        Mock.Mock<IMediaQueryCache>()
            .Setup(x => x.BuildCache())
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(new WarmupMediaQueryCacheCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        Mock.Mock<IMediaQueryCache>().Verify(
            x => x.GetMediaAsync(It.IsAny<MediaQueryFilter>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task ShouldReturnFailure_WhenMediaQueryCacheBuildFails()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Build cache failed");
        await SetupDatabase(8237);

        Mock.Mock<IMediaQueryCache>()
            .Setup(x => x.BuildCache())
            .ThrowsAsync(expectedException)
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(new WarmupMediaQueryCacheCommand(), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
        Mock.Mock<IMediaQueryCache>().Verify(
            x => x.GetMediaAsync(It.IsAny<MediaQueryFilter>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}