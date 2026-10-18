namespace Reaparr.Application.UnitTests;

public class RestartDownloadTaskCommandSourceScopeUnitTests : BaseCommandUnitTest<RestartDownloadTaskCommand>
{
    [Test]
    [Arguments(PlexMediaType.Movie, "server", false)]
    [Arguments(PlexMediaType.Movie, "library", false)]
    [Arguments(PlexMediaType.Movie, "media", false)]
    [Arguments(PlexMediaType.Episode, "server", false)]
    [Arguments(PlexMediaType.Episode, "library", false)]
    [Arguments(PlexMediaType.Episode, "media", false)]
    [Arguments(PlexMediaType.Movie, "server", true)]
    [Arguments(PlexMediaType.Movie, "library", true)]
    [Arguments(PlexMediaType.Movie, "media", true)]
    [Arguments(PlexMediaType.Episode, "server", true)]
    [Arguments(PlexMediaType.Episode, "library", true)]
    [Arguments(PlexMediaType.Episode, "media", true)]
    public async Task ShouldUseOnlyOriginalSourceOrMarkUnavailable_WhenMediaAndPartIdsCollide(
        PlexMediaType family,
        string differingScope,
        bool originalMissing
    )
    {
        // Arrange
        await SetupDatabase(88401, config =>
        {
            config.PlexServerCount = differingScope == "server" ? 2 : 1;
            config.PlexMovieLibraryCount = family == PlexMediaType.Movie && differingScope == "library" ? 2 : 0;
            config.PlexTvShowLibraryCount = family == PlexMediaType.Episode && differingScope == "library" ? 2 : 0;
            config.MovieCount = family == PlexMediaType.Movie ? (differingScope == "media" ? 2 : 1) : 0;
            config.TvShowCount = family == PlexMediaType.Episode ? (differingScope == "media" ? 2 : 1) : 0;
            config.TvShowSeasonCount = family == PlexMediaType.Episode ? 1 : 0;
            config.TvShowEpisodeCount = family == PlexMediaType.Episode ? 1 : 0;
            config.MovieDownloadTasksCount = family == PlexMediaType.Movie ? 2 : 0;
            config.TvShowDownloadTasksCount = family == PlexMediaType.Episode ? 2 : 0;
            config.TvShowSeasonDownloadTasksCount = family == PlexMediaType.Episode ? 1 : 0;
            config.TvShowEpisodeDownloadTasksCount = family == PlexMediaType.Episode ? 1 : 0;
        });
        var dbContext = IDbContext;
        BasePlexMediaData source;
        BasePlexMediaData competingSource;
        BasePlexMedia originalMedia;
        BasePlexMedia competingMedia;
        DownloadTaskBase root;
        DownloadTaskFileBase target;
        DownloadTaskFileBase control;
        if (family == PlexMediaType.Movie)
        {
            var sources = await dbContext.PlexMovieData.AsTracking().Include(x => x.PlexMovie)
                .OrderBy(x => x.Id).ToArrayAsync(CancellationToken);
            sources.Length.ShouldBe(2);
            competingSource = sources[0];
            source = sources[1];
            competingMedia = sources[0].PlexMovie!;
            originalMedia = sources[1].PlexMovie!;
            var tasks = await dbContext.DownloadTaskMovie.AsTracking().Include(x => x.Children)
                .OrderBy(x => x.Id).ToArrayAsync(CancellationToken);
            root = tasks[1];
            target = tasks[1].Children.Single();
            control = tasks[0].Children.Single();
        }
        else
        {
            var sources = await dbContext.PlexTvShowEpisodeData.AsTracking().Include(x => x.PlexTvShowEpisode)
                .ThenInclude(x => x!.TvShowSeason).ThenInclude(x => x!.TvShow)
                .OrderBy(x => x.Id).ToArrayAsync(CancellationToken);
            sources.Length.ShouldBe(2);
            competingSource = sources[0];
            source = sources[1];
            competingMedia = sources[0].PlexTvShowEpisode!;
            originalMedia = sources[1].PlexTvShowEpisode!;
            var tasks = await dbContext.DownloadTaskTvShow.AsTracking().Include(x => x.Children)
                .ThenInclude(x => x.Children).ThenInclude(x => x.Children)
                .OrderBy(x => x.Id).ToArrayAsync(CancellationToken);
            root = tasks[1];
            target = tasks[1].Children.Single().Children.Single().Children.Single();
            control = tasks[0].Children.Single().Children.Single().Children.Single();
        }

        if (differingScope != "media")
            dbContext.Entry(competingMedia).Property(nameof(BasePlexMedia.PlexApiRatingKey)).CurrentValue = originalMedia.PlexApiRatingKey;
        dbContext.Entry(source).CurrentValues.SetValues(new
        {
            PlexApiMediaId = 41001,
            PlexApiPartId = 42001,
            PlexApiRatingKey = originalMedia.PlexApiRatingKey,
            OriginalFilename = "original-source.mkv",
            GeneratedFilename = string.Empty,
            Key = "/library/parts/original/file.mkv",
            Size = 8192L,
        });
        dbContext.Entry(competingSource).CurrentValues.SetValues(new
        {
            PlexApiMediaId = 41001,
            PlexApiPartId = 42001,
            PlexApiRatingKey = competingMedia.PlexApiRatingKey,
            OriginalFilename = "unrelated-source.mkv",
            GeneratedFilename = string.Empty,
            Key = "/library/parts/unrelated/file.mkv",
            Size = 16384L,
        });
        new List<DownloadTaskBase> { root }.SetRelationshipIds(source.PlexServerId, source.PlexLibraryId);
        dbContext.Entry(target).CurrentValues.SetValues(new
        {
            PlexApiRatingKey = originalMedia.PlexApiRatingKey,
            PlexApiMediaId = 41001,
            PlexApiPartId = 42001,
            DownloadStatus = DownloadStatus.Stopped,
            FileName = "before-restart.mkv",
            FileLocationUrl = "/library/parts/before/file.mkv",
            DataTotal = 4096L,
            HashId = "preserved-hash",
        });
        await dbContext.SaveChangesAsync(CancellationToken);
        var key = target.ToKey();
        var controlBefore = (control.Id, control.DownloadStatus, control.FileName, control.FileLocationUrl, control.DataTotal);
        target.PlexServerId.ShouldBe(originalMedia.PlexServerId);
        target.PlexLibraryId.ShouldBe(originalMedia.PlexLibraryId);
        target.PlexApiRatingKey.ShouldBe(originalMedia.PlexApiRatingKey);
        competingSource.PlexApiMediaId.ShouldBe(target.PlexApiMediaId);
        competingSource.PlexApiPartId.ShouldBe(target.PlexApiPartId);
        if (originalMissing)
        {
            if (family == PlexMediaType.Movie)
                await dbContext.PlexMovieData.Where(x => x.Id == source.Id).ExecuteDeleteAsync(CancellationToken);
            else
                await dbContext.PlexTvShowEpisodeData.Where(x => x.Id == source.Id).ExecuteDeleteAsync(CancellationToken);
        }
        dbContext.ClearChangeTracker();
        SetupDependencies(builder => builder.RegisterType<DownloadTaskUpdateDispatcher>().As<IDownloadTaskUpdateDispatcher>());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.Is<StopDownloadTaskCommand>(command => command.DownloadTaskGuid == target.Id), CancellationToken))
            .ReturnsAsync(Result.Ok()).Verifiable(Times.Once());
        Mock.Mock<IEventPublisher>()
            .Setup(x => x.PublishAsync(It.Is<CheckDownloadQueueEvent>(message =>
                message.PlexServerIds.SequenceEqual(new[] { key.PlexServerId })), CancellationToken))
            .Returns(Task.CompletedTask).Verifiable(Times.Once());

        // Act
        var result = await TestHandlerExecuteAsync(new RestartDownloadTaskCommand(target.Id));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        var after = (await dbContext.GetDownloadTaskFileAsync(key, CancellationToken))!;
        after.Id.ShouldBe(target.Id);
        after.ToParentKey().ShouldBe(target.ToParentKey());
        after.PlexServerId.ShouldBe(originalMedia.PlexServerId);
        after.PlexLibraryId.ShouldBe(originalMedia.PlexLibraryId);
        after.PlexApiRatingKey.ShouldBe(originalMedia.PlexApiRatingKey);
        after.FileName.ShouldBe(originalMissing ? "before-restart.mkv" : "original-source.mkv");
        after.FileLocationUrl.ShouldBe(originalMissing ? "/library/parts/before/file.mkv" : "/library/parts/original/file.mkv");
        after.DataTotal.ShouldBe(originalMissing ? 4096 : 8192);
        after.DownloadStatus.ShouldBe(originalMissing ? DownloadStatus.SourceUnavailable : DownloadStatus.Queued);
        after.HashId.ShouldBe("preserved-hash");
        if (!originalMissing)
        {
            after.VideoQuality.ShouldBe(source.VideoQuality);
            after.FullTitle.ShouldBe($"{originalMedia.FullTitle}/original-source.mkv");
            if (family == PlexMediaType.Movie)
                after.DirectoryMeta.MovieFolder.ShouldBe(originalMedia.Title.SanitizeFolderName());
            else
            {
                var episode = (PlexTvShowEpisode)originalMedia;
                after.DirectoryMeta.TvShowFolder.ShouldBe(episode.TvShow!.Title.SanitizeFolderName());
                after.DirectoryMeta.SeasonFolder.ShouldBe(episode.TvShowSeason!.Title.SanitizeFolderName());
            }
        }
        var retained = (await dbContext.GetDownloadTaskFileAsync(control.ToKey(), CancellationToken))!;
        (retained.Id, retained.DownloadStatus, retained.FileName, retained.FileLocationUrl, retained.DataTotal)
            .ShouldBe(controlBefore);
        Mock.Mock<ICommandExecutor>().Verify();
        Mock.Mock<IEventPublisher>().Verify();
        Mock.Mock<ICommandExecutor>().Verify(x => x.Send(
            It.Is<StopDownloadTaskCommand>(command => command.DownloadTaskGuid != target.Id),
            It.IsAny<CancellationToken>()), Times.Never());
    }
}
