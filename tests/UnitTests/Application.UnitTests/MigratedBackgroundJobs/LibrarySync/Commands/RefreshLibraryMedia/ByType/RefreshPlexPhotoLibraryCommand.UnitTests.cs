namespace Reaparr.Application.UnitTests;

public class RefreshPlexPhotoLibraryCommandUnitTests : BaseCommandUnitTest<RefreshPlexPhotoLibraryCommand>
{
    [Test]
    public async Task ShouldRejectNonPhotoLibrariesBeforeSync()
    {
        await SetupDatabase(73101, x => x.PlexMovieLibraryCount = 1);
        var library = await IDbContext.PlexLibraries.SingleAsync(CancellationToken);
        var metadata = CreateMetadata(library);
        metadata.PlexLibrary.Type.ShouldBe(PlexMediaType.Movie);

        var result = await TestHandlerExecuteAsync<PlexLibrary>(new RefreshPlexPhotoLibraryCommand(metadata));

        result.IsFailed.ShouldBeTrue();
        result.ToResult().Has400BadRequestError().ShouldBeTrue();
        Mock.Mock<ICommandExecutor>().Verify(
            x => x.Send(It.IsAny<SyncPlexPhotosCommand>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        Mock.Mock<ILibrarySyncProgressStore>().Verify(
            x => x.StartAsync(It.IsAny<int>(), It.IsAny<PlexMediaType>(), It.IsAny<IReadOnlyDictionary<PlexMediaType, int>?>()),
            Times.Never
        );
    }

    [Test]
    public async Task ShouldPropagateSyncFailureWithoutPublishingProgress()
    {
        await SetupDatabase(73102, x => x.PlexPhotoLibraryCount = 1);
        var library = await IDbContext.PlexLibraries.SingleAsync(CancellationToken);
        var metadata = CreateMetadata(library);
        var failure = Result.Fail<CrudPhotosReport>("catalog reconciliation failed");

        Mock.Mock<ILibrarySyncProgressStore>()
            .Setup(x => x.StartAsync(library.Id, PlexMediaType.PhotoAlbum, null))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.Is<SyncPlexPhotosCommand>(c => ReferenceEquals(c.LibraryMetadata, metadata)), CancellationToken))
            .ReturnsAsync(failure)
            .Verifiable(Times.Once());
        Mock.Mock<ILibrarySyncProgressStore>()
            .Setup(x => x.UpdateErrorAsync(library.Id, It.Is<Result>(r => r.IsFailed), CancellationToken))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        var result = await TestHandlerExecuteAsync<PlexLibrary>(new RefreshPlexPhotoLibraryCommand(metadata));

        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(x => x.Message == "catalog reconciliation failed");
        Mock.Mock<ICommandExecutor>().Verify(
            x => x.Send(It.IsAny<ScheduleOptimizeDatabaseJobCommand>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        Mock.Mock<ICommandExecutor>().Verify(
            x => x.Send(It.IsAny<QueueMediaOverviewRebuildCommand>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        Mock.Mock<ILibrarySyncProgressStore>().Verify();
    }

    [Test]
    public async Task ShouldSyncGraphPublishProgressAndQueueOverviewRebuild()
    {
        await SetupDatabase(73103, x => x.PlexPhotoLibraryCount = 1);
        var library = await IDbContext.PlexLibraries.SingleAsync(CancellationToken);
        var metadata = CreateMetadata(library, includePhoto: true);
        var report = new CrudPhotosReport { CreatedPhotoAlbums = 1, CreatedPhotos = 1 };

        Mock.Mock<ILibrarySyncProgressStore>()
            .Setup(x => x.StartAsync(library.Id, PlexMediaType.PhotoAlbum, null))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.Is<SyncPlexPhotosCommand>(c => ReferenceEquals(c.LibraryMetadata, metadata)), CancellationToken))
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
                    && p.Received == metadata.PlexLibrary.PhotoAlbums.Count
                    && p.Total == metadata.PlexLibrary.PhotoAlbums.Count
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
                    && p.Received == metadata.PlexLibrary.PhotoMediaData.Count
                    && p.Total == metadata.PlexLibrary.PhotoMediaData.Count
                    && p.TimeRemaining == TimeSpan.Zero
                ),
                CancellationToken
            ))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        var result = await TestHandlerExecuteAsync<PlexLibrary>(new RefreshPlexPhotoLibraryCommand(metadata));

        result.IsSuccess.ShouldBeTrue(result.ToString());
        result.Value.Id.ShouldBe(library.Id);
        Mock.Mock<ICommandExecutor>().Verify();
        Mock.Mock<ILibrarySyncProgressStore>().Verify();
    }

    [Test]
    public async Task ShouldPropagateCancelledSyncWithoutPublication()
    {
        await SetupDatabase(73104, x => x.PlexPhotoLibraryCount = 1);
        var library = await IDbContext.PlexLibraries.SingleAsync(CancellationToken);
        var metadata = CreateMetadata(library);
        var cancelled = ResultExtensions.TaskIsCancelled("photo reconciliation");

        Mock.Mock<ILibrarySyncProgressStore>()
            .Setup(x => x.StartAsync(library.Id, PlexMediaType.PhotoAlbum, null))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.Is<SyncPlexPhotosCommand>(c => ReferenceEquals(c.LibraryMetadata, metadata)), CancellationToken))
            .ReturnsAsync(cancelled)
            .Verifiable(Times.Once());

        var result = await TestHandlerExecuteAsync<PlexLibrary>(new RefreshPlexPhotoLibraryCommand(metadata));

        result.IsCancelled.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>().Verify(
            x => x.Send(It.IsAny<QueueMediaOverviewRebuildCommand>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        Mock.Mock<ILibrarySyncProgressStore>().Verify(
            x => x.UpdateItemAsync(It.IsAny<int>(), It.IsAny<LibraryProgressItem>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    private static InsertMediaMetaDataCommandResponse CreateMetadata(PlexLibrary databaseLibrary, bool includePhoto = false)
    {
        var library = FakeData.GetPlexLibrary(new Seed(databaseLibrary.Id + 100), databaseLibrary.Type).Generate();
        library.Id = databaseLibrary.Id;
        library.PlexServerId = databaseLibrary.PlexServerId;
        library.PhotoAlbums.Clear();
        library.PhotoMediaData.Clear();
        library.PhotoFolders.Clear();
        if (includePhoto)
        {
            var root = CreateRoot();
            var album = CreateAlbum(501);
            var file = CreateFile(601);
            root.Files.Add(file);
            file.PlexPhotoFolder = root;
            album.Memberships.Add(new PlexPhotoAlbumMembership
            {
                PlexPhotoAlbumId = 0,
                PlexPhotoMediaDataId = 0,
                PlexPhotoAlbum = album,
                PlexPhotoMediaData = file,
            });
            library.PhotoFolders.Add(root);
            library.PhotoAlbums.Add(album);
            library.PhotoMediaData.Add(file);
        }

        return new InsertMediaMetaDataCommandResponse(library)
        {
            PlexActors = [],
            PlexGenresVideo = [],
            PlexGenresAudio = [],
            PlexCountries = [],
        };
    }

    private static PlexPhotoFolder CreateRoot() =>
        new()
        {
            Id = 0,
            PlexServerId = 1,
            PlexLibraryId = 1,
            ParentId = null,
            RelativePath = string.Empty,
            Title = "photos",
            RootIdentityKey = "path:posix:/fixture/photos",
            SourceRootPath = "/fixture/photos",
            IsActive = true,
        };

    private static PlexPhotoAlbum CreateAlbum(int ratingKey) =>
        new()
        {
            Id = 0,
            PlexServerId = 1,
            PlexLibraryId = 1,
            PlexApiRatingKey = ratingKey,
            Title = "Album",
            SearchTitle = "album",
            Summary = null,
            PlexApiMetaDataKey = ratingKey,
            HasThumb = false,
            AddedAt = DateTime.UtcNow,
            UpdatedAt = null,
        };

    private static PlexPhotoMediaData CreateFile(int ratingKey) =>
        new()
        {
            Id = 0,
            PlexServerId = 1,
            PlexLibraryId = 1,
            PlexApiRatingKey = ratingKey,
            PlexApiMediaId = 701,
            PlexApiPartId = 702,
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
            Key = "/library/parts/702/file.jpg",
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
