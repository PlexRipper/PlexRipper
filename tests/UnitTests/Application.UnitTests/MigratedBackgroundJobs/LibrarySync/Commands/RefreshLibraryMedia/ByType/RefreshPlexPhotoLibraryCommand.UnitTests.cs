namespace Reaparr.Application.UnitTests;

public class RefreshPlexPhotoLibraryCommandUnitTests : BaseCommandUnitTest<RefreshPlexPhotoLibraryCommand>
{
    [Test]
    public async Task ShouldRejectNonPhotoLibrariesBeforeRetrieval()
    {
        // Arrange
        await SetupDatabase(73101, x => x.PlexMovieLibraryCount = 1);
        var library = await IDbContext.PlexLibraries.SingleAsync(CancellationToken);
        var originalType = library.Type;

        // Act
        var result = await TestHandlerExecuteAsync<PlexLibrary>(new RefreshPlexPhotoLibraryCommand(library));

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.IsCancelled.ShouldBeFalse();
        result.ToResult().Has400BadRequestError().ShouldBeTrue();
        result.Errors.ShouldContain(x => x.Message.Contains(nameof(PlexMediaType.PhotoAlbum), StringComparison.Ordinal));
        result.ValueOrDefault.ShouldBeNull();
        library.Type.ShouldBe(originalType);
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<GetPhotoLibrarySnapshotFromPlexApiCommand>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        Mock.Mock<ILibrarySyncProgressStore>()
            .Verify(
                x => x.StartAsync(It.IsAny<int>(), It.IsAny<PlexMediaType>(), It.IsAny<IReadOnlyDictionary<PlexMediaType, int>?>()),
                Times.Never
            );
    }

    [Test]
    public async Task ShouldPreserveCatalogAndReportRetrievalFailure()
    {
        // Arrange
        await SetupDatabase(73102, x =>
        {
            x.PlexPhotoLibraryCount = 1;
            x.PhotoAlbumCount = 1;
            x.PhotoCount = 1;
        });
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(CancellationToken);
        var beforeCounts = await dbContext
            .PlexPhotoMediaData.OrderBy(x => x.Id)
            .Select(x => new { x.Id, x.PlexApiRatingKey, x.Key })
            .ToListAsync(CancellationToken);
        var failure = Result.Fail<PhotoLibrarySnapshot>("catalog retrieval incomplete");
        Mock.Mock<ILibrarySyncProgressStore>()
            .Setup(x => x.StartAsync(library.Id, PlexMediaType.PhotoAlbum, null))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.Is<GetPhotoLibrarySnapshotFromPlexApiCommand>(c => c.PlexLibrary == library), CancellationToken))
            .ReturnsAsync(failure)
            .Verifiable(Times.Once());
        Mock.Mock<ILibrarySyncProgressStore>()
            .Setup(x => x.UpdateErrorAsync(library.Id, It.Is<Result>(r => r.IsFailed && r.Errors.Count == 1), CancellationToken))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        // Act
        var result = await TestHandlerExecuteAsync<PlexLibrary>(new RefreshPlexPhotoLibraryCommand(library));

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.IsCancelled.ShouldBeFalse();
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].Message.ShouldBe(failure.Errors[0].Message);
        result.ValueOrDefault.ShouldBeNull();
        var afterCounts = await dbContext
            .PlexPhotoMediaData.OrderBy(x => x.Id)
            .Select(x => new { x.Id, x.PlexApiRatingKey, x.Key })
            .ToListAsync(CancellationToken);
        afterCounts.ShouldBe(beforeCounts);
        Mock.Mock<ICommandExecutor>().Verify(x => x.Send(It.IsAny<SyncPlexPhotosCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        Mock.Mock<ICommandExecutor>().Verify(x => x.Send(It.IsAny<ScheduleOptimizeDatabaseJobCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        Mock.Mock<ICommandExecutor>().Verify(x => x.Send(It.IsAny<QueueMediaOverviewRebuildCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        Mock.Mock<ICommandExecutor>().Verify();
        Mock.Mock<ILibrarySyncProgressStore>().Verify();
    }

    [Test]
    public async Task ShouldSyncSnapshotPublishProgressAndQueueOverviewRebuild()
    {
        // Arrange
        await SetupDatabase(73103, x => x.PlexPhotoLibraryCount = 1);
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(CancellationToken);
        var payload = FakeData.GetPlexLibrary(new Seed(73103), PlexMediaType.PhotoAlbum).Generate();
        payload.Id = library.Id;
        payload.PlexServerId = library.PlexServerId;
        var album = CreateAlbum(501, "Album");
        var file = CreateFile(601, 701);
        payload.PhotoAlbums.Add(album);
        payload.PhotoMediaData.Add(file);
        var root = CreateRootInput("/fixture/photos");
        var snapshot = new PhotoLibrarySnapshot(payload, [root], [new(501, 601)]);
        var report = new CrudPhotosReport { CreatedPhotoAlbums = 1, CreatedPhotos = 1 };

        Mock.Mock<ILibrarySyncProgressStore>()
            .Setup(x => x.StartAsync(library.Id, PlexMediaType.PhotoAlbum, null))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.Is<GetPhotoLibrarySnapshotFromPlexApiCommand>(c => c.PlexLibrary == library), CancellationToken))
            .ReturnsAsync(Result.Ok(snapshot))
            .Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.Is<SyncPlexPhotosCommand>(c => c.Snapshot == snapshot && !c.ForceMediaRefresh), CancellationToken))
            .ReturnsAsync(Result.Ok(report))
            .Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.Is<ScheduleOptimizeDatabaseJobCommand>(c => c.ChangedItemCount == report.ChangedItemCount), CancellationToken))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());
        Mock.SetupCommand(() => new QueueMediaOverviewRebuildCommand()).ReturnsAsync(Result.Ok()).Verifiable(Times.Once());
        Mock.Mock<ILibrarySyncProgressStore>()
            .Setup(x => x.UpdateItemAsync(
                library.Id,
                It.Is<LibraryProgressItem>(p =>
                    p.MediaType == PlexMediaType.PhotoAlbum
                    && p.Received == snapshot.PlexLibrary.PhotoAlbums.Count
                    && p.Total == snapshot.PlexLibrary.PhotoAlbums.Count
                    && p.TimeRemaining == TimeSpan.Zero
                ),
                CancellationToken
            ))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());
        Mock.Mock<ILibrarySyncProgressStore>()
            .Setup(x => x.UpdateItemAsync(
                library.Id,
                It.Is<LibraryProgressItem>(p =>
                    p.MediaType == PlexMediaType.PhotoImage
                    && p.Received == snapshot.PlexLibrary.PhotoMediaData.Count
                    && p.Total == snapshot.PlexLibrary.PhotoMediaData.Count
                    && p.TimeRemaining == TimeSpan.Zero
                ),
                CancellationToken
            ))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        // Act
        var result = await TestHandlerExecuteAsync<PlexLibrary>(new RefreshPlexPhotoLibraryCommand(library));

        // Assert
        result.IsSuccess.ShouldBeTrue(result.ToString());
        result.Errors.Count.ShouldBe(0);
        result.Value.Id.ShouldBe(library.Id);
        result.Value.Type.ShouldBe(PlexMediaType.PhotoAlbum);
        Mock.Mock<ICommandExecutor>().Verify();
        Mock.Mock<ILibrarySyncProgressStore>().Verify();
    }

    [Test]
    public async Task ShouldPropagateCancelledRetrievalWithoutSyncOrPublication()
    {
        // Arrange
        await SetupDatabase(73104, x => x.PlexPhotoLibraryCount = 1);
        var library = await IDbContext.PlexLibraries.SingleAsync(CancellationToken);
        var cancelled = ResultExtensions.TaskIsCancelled("photo retrieval");
        Mock.Mock<ILibrarySyncProgressStore>()
            .Setup(x => x.StartAsync(library.Id, PlexMediaType.PhotoAlbum, null))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.Is<GetPhotoLibrarySnapshotFromPlexApiCommand>(c => c.PlexLibrary == library), CancellationToken))
            .ReturnsAsync(cancelled)
            .Verifiable(Times.Once());

        // Act
        var result = await TestHandlerExecuteAsync<PlexLibrary>(new RefreshPlexPhotoLibraryCommand(library));

        // Assert
        result.IsCancelled.ShouldBeTrue();
        result.Errors.Count.ShouldBe(cancelled.Errors.Count);
        result.ValueOrDefault.ShouldBeNull();
        Mock.Mock<ICommandExecutor>().Verify(x => x.Send(It.IsAny<SyncPlexPhotosCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        Mock.Mock<ICommandExecutor>().Verify(x => x.Send(It.IsAny<QueueMediaOverviewRebuildCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        Mock.Mock<ILibrarySyncProgressStore>().Verify(x => x.UpdateItemAsync(It.IsAny<int>(), It.IsAny<LibraryProgressItem>(), It.IsAny<CancellationToken>()), Times.Never);
        Mock.Mock<ICommandExecutor>().Verify();
        Mock.Mock<ILibrarySyncProgressStore>().Verify();
    }

    private static PhotoSourceRootInput CreateRootInput(string path)
    {
        PhotoSourcePathResolver.TryCreateIdentityKey(path, out var identity, out var error).ShouldBeTrue(error);
        return new PhotoSourceRootInput(null, identity, path);
    }

    private static PlexPhotoAlbum CreateAlbum(int ratingKey, string title) =>
        new()
        {
            Id = 0,
            PlexServerId = 1,
            PlexLibraryId = 1,
            PlexApiRatingKey = ratingKey,
            Title = title,
            SearchTitle = title.ToSearchTitle(),
            Summary = null,
            PlexApiMetaDataKey = ratingKey,
            HasThumb = false,
            AddedAt = DateTime.UtcNow,
            UpdatedAt = null,
        };

    private static PlexPhotoMediaData CreateFile(int ratingKey, int partId) =>
        new()
        {
            Id = 0,
            PlexServerId = 1,
            PlexLibraryId = 1,
            PlexApiRatingKey = ratingKey,
            PlexApiMediaId = partId + 100,
            PlexApiPartId = partId,
            AssetKind = PhotoAssetKind.Image,
            Title = "file.jpg",
            SearchTitle = "file.jpg",
            Summary = null,
            PlexApiMetaDataKey = ratingKey,
            HasThumb = false,
            AddedAt = DateTime.UtcNow,
            UpdatedAt = null,
            OriginallyAvailableAt = null,
            SourcePath = "/fixture/photos/file.jpg",
            OriginalFilename = "file.jpg",
            Key = $"/library/parts/{partId}/file.jpg",
            Size = 20,
            Container = "jpg",
            Width = 100,
            Height = 100,
            Duration = null,
            VideoCodec = null,
            AudioCodec = null,
            PlexPhotoFolderId = null,
        };
}
