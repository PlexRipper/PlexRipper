namespace Reaparr.Application.UnitTests;

public class RefreshDownloadTaskMetadataCommandHandlerUnitTests
    : BaseUnitTest<RefreshDownloadTaskMetadataCommandHandler>
{
    [Test]
    [Arguments("Top.Gear.(2002).S22E04.mkv", 22, 4)]
    [Arguments("The X-Files.S03.E15 - Piper Maru.mkv", 3, 15)]
    [Arguments("top_gear.23x01.720p_hdtv_x264-fov.mkv", 23, 1)]
    [Arguments("The Sopranos/Season 2/The Happy Wanderer/The.Sopranos.S02E06.mkv", 2, 6)]
    [Arguments("Show/Season 5/Episode 03.mkv", 5, 3)]
    public void ShouldParseSeasonEpisode_FromCommonFilenamePatterns(string fullTitle, int season, int episode)
    {
        var parsed = RefreshDownloadTaskMetadataCommandHandler.ParseSeasonEpisode(fullTitle);
        parsed.ShouldNotBeNull();
        parsed.Value.Season.ShouldBe(season);
        parsed.Value.Episode.ShouldBe(episode);
    }

    [Test]
    public async Task ShouldRespectCooldown_OnRepeatedRefreshAttempts()
    {
        // Arrange — clear the static dict so this test is deterministic.
        RefreshDownloadTaskMetadataCommandHandler._lastRefreshAt.Clear();
        await SetupDatabase(76001, config =>
        {
            config.PlexServerCount = 1;
            config.PlexMovieLibraryCount = 1;
            config.MovieCount = 1;
            config.MovieDownloadTasksCount = 1;
        });

        var taskId = (await IDbContext.DownloadTaskMovieFile.AsNoTracking().FirstAsync(CancellationToken)).Id;

        // Act — first call should run; second within cooldown should short-circuit.
        var first = await Sut.ExecuteAsync(
            new RefreshDownloadTaskMetadataCommand(taskId, DownloadTaskType.MovieData),
            CancellationToken
        );
        var second = await Sut.ExecuteAsync(
            new RefreshDownloadTaskMetadataCommand(taskId, DownloadTaskType.MovieData),
            CancellationToken
        );

        // Assert — both succeed; both return false (no work because seeded data has matching IDs).
        first.IsSuccess.ShouldBeTrue();
        second.IsSuccess.ShouldBeTrue();
        first.Value.ShouldBeFalse();
        second.Value.ShouldBeFalse();
        RefreshDownloadTaskMetadataCommandHandler._lastRefreshAt.ContainsKey(taskId).ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnFalse_WhenTaskNotFound()
    {
        RefreshDownloadTaskMetadataCommandHandler._lastRefreshAt.Clear();
        await SetupDatabase(76002);

        var result = await Sut.ExecuteAsync(
            new RefreshDownloadTaskMetadataCommand(Guid.NewGuid(), DownloadTaskType.MovieData),
            CancellationToken
        );

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldReturnFalse_WhenTaskTypeIsUnsupported()
    {
        RefreshDownloadTaskMetadataCommandHandler._lastRefreshAt.Clear();
        await SetupDatabase(76003);

        var result = await Sut.ExecuteAsync(
            new RefreshDownloadTaskMetadataCommand(Guid.NewGuid(), DownloadTaskType.Season),
            CancellationToken
        );

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeFalse();
    }
}
