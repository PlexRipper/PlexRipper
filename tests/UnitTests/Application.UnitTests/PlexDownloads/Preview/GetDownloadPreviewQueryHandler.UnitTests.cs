namespace Reaparr.Application.UnitTests;

public class GetDownloadPreviewQueryHandlerUnitTests : BaseCommandUnitTest<GetDownloadPreviewQuery>
{
    [Test]
    public async Task ShouldBuildPhotoFolderAndFilePreview_WhenDirectAssetIsSelected()
    {
        // Arrange
        await SetupDatabase(
            62401,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexPhotoLibraryCount = 1;
                config.PhotoCount = 1;
            }
        );
        var dbContext = IDbContext;
        var media = await dbContext.PlexPhotoMediaData.SingleAsync(CancellationToken);
        var query = new GetDownloadPreviewQuery(
            [
                new DownloadMediaDTO
                {
                    Type = PlexMediaType.PhotoImage,
                    PlexServerId = media.PlexServerId,
                    PlexLibraryId = media.PlexLibraryId,
                    MediaIds = [media.Id],
                    Qualities = [],
                },
            ]
        );

        // Act
        var result = await TestHandlerExecuteAsync<List<DownloadPreview>>(query);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        result.Value.Count.ShouldBe(1);
        var rootPreview = result.Value.Single();
        rootPreview.MediaType.ShouldBe(PlexMediaType.PhotoAlbum);
        rootPreview.Size.ShouldBe(media.Size);
        rootPreview.ChildCount.ShouldBe(1);
        rootPreview.Children.Count.ShouldBe(1);
        var filePreview = rootPreview.Children.Single();
        filePreview.MediaType.ShouldBe(PlexMediaType.PhotoImage);
        filePreview.Id.ShouldBe(media.Id);
        filePreview.Title.ShouldBe(media.OriginalFilename.GetFileName());
        filePreview.Size.ShouldBe(media.Size);
        filePreview.ChildCount.ShouldBe(0);
        filePreview.Qualities.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldPreservePhysicalFolderHierarchy_WhenAssetIsMappedToDescendantFolder()
    {
        // Arrange
        await SetupDatabase(
            62402,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexPhotoLibraryCount = 1;
                config.PhotoCount = 1;
            }
        );
        var dbContext = IDbContext;
        var root = await dbContext.PlexPhotoFolders.SingleAsync(x => x.ParentId == null, CancellationToken);
        var media = await dbContext.PlexPhotoMediaData.AsTracking().SingleAsync(CancellationToken);
        var child = new PlexPhotoFolder
        {
            PlexServerId = root.PlexServerId,
            PlexLibraryId = root.PlexLibraryId,
            ParentId = root.Id,
            RelativePath = "Trips",
            Title = "Trips",
        };
        dbContext.PlexPhotoFolders.Add(child);
        media.PlexPhotoFolder = child;
        await dbContext.SaveChangesAsync(CancellationToken);
        var query = new GetDownloadPreviewQuery(
            [
                new DownloadMediaDTO
                {
                    Type = PlexMediaType.PhotoImage,
                    PlexServerId = media.PlexServerId,
                    PlexLibraryId = media.PlexLibraryId,
                    MediaIds = [media.Id],
                    Qualities = [],
                },
            ]
        );

        // Act
        var result = await TestHandlerExecuteAsync<List<DownloadPreview>>(query);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        var folderPreview = result.Value.Single().Children.Single();
        folderPreview.MediaType.ShouldBe(PlexMediaType.PhotoAlbum);
        folderPreview.Title.ShouldBe("Trips");
        folderPreview.Size.ShouldBe(media.Size);
        folderPreview.ChildCount.ShouldBe(1);
        folderPreview.Children.Single().Title.ShouldBe(media.OriginalFilename.GetFileName());
    }

    [Test]
    public async Task ShouldExpandAlbumAndDirectSelectionToOnePhysicalFilePreview()
    {
        // Arrange
        await SetupDatabase(
            62403,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexPhotoLibraryCount = 1;
                config.PhotoAlbumCount = 1;
                config.PhotoCount = 1;
            }
        );
        var dbContext = IDbContext;
        var album = await dbContext.PlexPhotoAlbums.SingleAsync(CancellationToken);
        var media = await dbContext.PlexPhotoMediaData.SingleAsync(CancellationToken);
        var query = new GetDownloadPreviewQuery(
            [
                new DownloadMediaDTO
                {
                    Type = PlexMediaType.PhotoAlbum,
                    PlexServerId = album.PlexServerId,
                    PlexLibraryId = album.PlexLibraryId,
                    MediaIds = [album.Id],
                    Qualities = [],
                },
                new DownloadMediaDTO
                {
                    Type = PlexMediaType.PhotoImage,
                    PlexServerId = media.PlexServerId,
                    PlexLibraryId = media.PlexLibraryId,
                    MediaIds = [media.Id],
                    Qualities = [],
                },
            ]
        );

        // Act
        var result = await TestHandlerExecuteAsync<List<DownloadPreview>>(query);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        result.Value.Count.ShouldBe(1);
        result.Value.Single().Children.Count.ShouldBe(1);
        result.Value.Single().Children.Single().MediaType.ShouldBe(PlexMediaType.PhotoImage);
    }

    [Test]
    public async Task ShouldRejectInheritedKeepCompletedConflict_BeforeBuildingPreview()
    {
        // Arrange
        await SetupDatabase(
            62404,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexPhotoLibraryCount = 1;
                config.PhotoAlbumCount = 2;
                config.PhotoCount = 1;
            }
        );
        var dbContext = IDbContext;
        var albums = await dbContext.PlexPhotoAlbums.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var media = await dbContext.PlexPhotoMediaData.OrderBy(x => x.Id).FirstAsync(CancellationToken);
        dbContext.PlexPhotoAlbumMemberships.Add(
            new PlexPhotoAlbumMembership { PlexPhotoAlbumId = albums[1].Id, PlexPhotoMediaDataId = media.Id }
        );
        await dbContext.SaveChangesAsync(CancellationToken);
        var query = new GetDownloadPreviewQuery(
            albums
                .Select(
                    (album, index) => new DownloadMediaDTO
                    {
                        Type = PlexMediaType.PhotoAlbum,
                        PlexServerId = album.PlexServerId,
                        PlexLibraryId = album.PlexLibraryId,
                        MediaIds = [album.Id],
                        Qualities = [],
                        KeepCompletedInDownloadFolder = index == 0,
                    }
                )
                .ToList()
        );

        // Act
        var result = await TestHandlerExecuteAsync<List<DownloadPreview>>(query);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.ValueOrDefault.ShouldBeNull();
    }

    [Test]
    [Arguments(PlexMediaType.MusicTrack)]
    [Arguments(PlexMediaType.OtherVideos)]
    public async Task ShouldRejectWrongMediaScope_WhenNonPhotoSelectionUsesAnotherServer(PlexMediaType type)
    {
        // Arrange
        await SetupDatabase(
            62830,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMusicLibraryCount = type == PlexMediaType.MusicTrack ? 1 : 0;
                config.MusicArtistCount = type == PlexMediaType.MusicTrack ? 1 : 0;
                config.MusicAlbumCount = type == PlexMediaType.MusicTrack ? 1 : 0;
                config.MusicTrackCount = type == PlexMediaType.MusicTrack ? 2 : 0;
                config.PlexOtherVideoLibraryCount = type == PlexMediaType.OtherVideos ? 1 : 0;
                config.OtherVideoCount = type == PlexMediaType.OtherVideos ? 2 : 0;
            }
        );
        var dbContext = IDbContext;
        List<BasePlexMedia> media = type == PlexMediaType.MusicTrack
            ? [.. await dbContext.PlexMusicTracks.OrderBy(x => x.Id).ToListAsync(CancellationToken)]
            : [.. await dbContext.PlexOtherVideos.OrderBy(x => x.Id).ToListAsync(CancellationToken)];
        var target = media[0];
        var foreignServerId = await dbContext.PlexServers.Where(x => x.Id != target.PlexServerId)
            .Select(x => x.Id).FirstAsync(CancellationToken);
        var query = new GetDownloadPreviewQuery(
            [
                new DownloadMediaDTO
                {
                    Type = type,
                    PlexServerId = foreignServerId,
                    PlexLibraryId = target.PlexLibraryId,
                    MediaIds = [target.Id],
                    Qualities = [],
                },
            ]
        );

        // Act
        var result = await TestHandlerExecuteAsync<List<DownloadPreview>>(query);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.ToResult().Has400BadRequestError().ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnNoPreview_WhenEmptyListIsGiven()
    {
        // Arrange
        await SetupDatabase(15140);

        // Act
        var result = await new GetDownloadPreviewQueryHandler(IDbContext, Log).ExecuteAsync(
            new GetDownloadPreviewQuery([]),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Test]
    [Arguments(PlexMediaType.MusicArtist)]
    [Arguments(PlexMediaType.MusicAlbum)]
    [Arguments(PlexMediaType.MusicTrack)]
    public async Task ShouldReturnTypedAncestorsAndSelectedDescendants_WhenMusicIsRequested(PlexMediaType type)
    {
        // Arrange
        await SetupDatabase(
            62801,
            config =>
            {
                config.PlexMusicLibraryCount = 1;
                config.MusicArtistCount = 2;
                config.MusicAlbumCount = 2;
                config.MusicTrackCount = 2;
            }
        );
        var artist = await IDbContext
            .PlexMusicArtists.Include(x => x.Albums)
                .ThenInclude(x => x.Tracks)
                    .ThenInclude(x => x.MediaDataList)
            .OrderBy(x => x.Id)
            .FirstAsync(CancellationToken);
        var album = artist.Albums.OrderBy(x => x.Id).First();
        var track = album.Tracks.OrderBy(x => x.Id).First();
        var id = type switch
        {
            PlexMediaType.MusicArtist => artist.Id,
            PlexMediaType.MusicAlbum => album.Id,
            _ => track.Id,
        };
        var query = new GetDownloadPreviewQuery(
            [
                new DownloadMediaDTO
                {
                    Type = type,
                    PlexServerId = artist.PlexServerId,
                    PlexLibraryId = artist.PlexLibraryId,
                    MediaIds = [id],
                    Qualities = [],
                },
            ]
        );

        // Act
        var result = await TestHandlerExecuteAsync<List<DownloadPreview>>(query);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        var previewArtist = result.Value.Single();
        previewArtist.MediaType.ShouldBe(PlexMediaType.MusicArtist);
        previewArtist.Children.ShouldNotBeEmpty();
        previewArtist.Size.ShouldBeGreaterThan(0);
        if (type == PlexMediaType.MusicTrack)
            previewArtist.Children.SelectMany(x => x.Children).Count().ShouldBe(1);
        else
            previewArtist.Children.SelectMany(x => x.Children).ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldReturnMoviePreviewWithSelectedQualities()
    {
        // Arrange
        await SetupDatabase(
            23456,
            config =>
            {
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 2;
            }
        );
        var movies = await IDbContext
            .PlexMovies.Include(x => x.MediaDataList)
            .AsNoTracking()
            .ToListAsync(CancellationToken);
        var query = new GetDownloadPreviewQuery(
            [
                new DownloadMediaDTO
                {
                    Type = PlexMediaType.Movie,
                    PlexServerId = movies[0].PlexServerId,
                    PlexLibraryId = movies[0].PlexLibraryId,
                    MediaIds = movies.Select(x => x.Id).ToList(),
                    Qualities = movies
                        .SelectMany(x =>
                            x.MediaDataList.Take(1).Select(data => new PlexMediaQualityDTO
                            {
                                MediaId = x.Id,
                                DataId = data.Id,
                                MediaDataType = PlexMediaType.Movie,
                                VideoQuality = VideoQuality.UHD_4K,
                            })
                        )
                        .ToList(),
                },
            ]
        );

        // Act
        var result = await TestHandlerExecuteAsync<List<DownloadPreview>>(query);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);
        result.Value.ShouldAllBe(x => x.MediaType == PlexMediaType.Movie);
        result.Value.ShouldAllBe(x => x.Qualities.Count == 1);
        result.Value.ShouldAllBe(x => x.Size > 0);
    }

    [Test]
    public async Task ShouldReturnOtherVideoPreviewWithSelectedQualities()
    {
        // Arrange
        await SetupDatabase(
            23901,
            config =>
            {
                config.PlexOtherVideoLibraryCount = 1;
                config.OtherVideoCount = 2;
            }
        );
        var videos = await IDbContext
            .PlexOtherVideos.Include(x => x.MediaDataList)
            .AsNoTracking()
            .ToListAsync(CancellationToken);
        var query = new GetDownloadPreviewQuery(
            [
                new DownloadMediaDTO
                {
                    Type = PlexMediaType.OtherVideos,
                    PlexServerId = videos[0].PlexServerId,
                    PlexLibraryId = videos[0].PlexLibraryId,
                    MediaIds = videos.Select(x => x.Id).ToList(),
                    Qualities = videos
                        .SelectMany(x =>
                            x.MediaDataList.Take(1).Select(data => new PlexMediaQualityDTO
                            {
                                MediaId = x.Id,
                                DataId = data.Id,
                                MediaDataType = PlexMediaType.OtherVideos,
                                VideoQuality = VideoQuality.HD,
                            })
                        )
                        .ToList(),
                },
            ]
        );

        // Act
        var result = await TestHandlerExecuteAsync<List<DownloadPreview>>(query);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);
        result.Value.ShouldAllBe(x => x.MediaType == PlexMediaType.OtherVideos);
        result.Value.ShouldAllBe(x => x.Qualities.Count == 1);
        result.Value.ShouldAllBe(x => x.Size > 0);
    }

    [Test]
    public async Task ShouldBuildMixedMovieAndTvHierarchy()
    {
        // Arrange
        await SetupDatabase(
            47561,
            config =>
            {
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 3;
                config.PlexTvShowLibraryCount = 1;
                config.TvShowCount = 2;
                config.TvShowSeasonCount = 2;
                config.TvShowEpisodeCount = 2;
            }
        );
        var dbContext = IDbContext;
        var movies = await dbContext.PlexMovies.ToListAsync(CancellationToken);
        var shows = await dbContext.PlexTvShows.ToListAsync(CancellationToken);
        var query = new GetDownloadPreviewQuery(
            [
                new DownloadMediaDTO
                {
                    Type = PlexMediaType.Movie,
                    PlexServerId = movies[0].PlexServerId,
                    PlexLibraryId = movies[0].PlexLibraryId,
                    MediaIds = movies.Select(x => x.Id).ToList(),
                    Qualities = [],
                },
                new DownloadMediaDTO
                {
                    Type = PlexMediaType.TvShow,
                    PlexServerId = shows[0].PlexServerId,
                    PlexLibraryId = shows[0].PlexLibraryId,
                    MediaIds = shows.Select(x => x.Id).ToList(),
                    Qualities = [],
                },
            ]
        );

        // Act
        var result = await TestHandlerExecuteAsync<List<DownloadPreview>>(query);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(5);
        result.Value.Count(x => x.MediaType == PlexMediaType.Movie).ShouldBe(3);
        result.Value.Count(x => x.MediaType == PlexMediaType.TvShow).ShouldBe(2);
        result.Value.Where(x => x.MediaType == PlexMediaType.TvShow).ShouldAllBe(x =>
            x.Children.Count == 2 && x.Children.All(season => season.Children.Count == 2)
        );
    }

    [Test]
    public async Task ShouldHandleMultipleMovieLibrariesAndServers()
    {
        // Arrange
        await SetupDatabase(
            11223,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMovieLibraryCount = 2;
                config.MovieCount = 2;
            }
        );
        var movies = await IDbContext.PlexMovies.AsNoTracking().ToListAsync(CancellationToken);
        var query = new GetDownloadPreviewQuery(
            movies
                .GroupBy(x => (x.PlexServerId, x.PlexLibraryId))
                .Select(group => new DownloadMediaDTO
                {
                    Type = PlexMediaType.Movie,
                    PlexServerId = group.Key.PlexServerId,
                    PlexLibraryId = group.Key.PlexLibraryId,
                    MediaIds = group.Select(x => x.Id).ToList(),
                    Qualities = [],
                })
                .ToList()
        );

        // Act
        var result = await TestHandlerExecuteAsync<List<DownloadPreview>>(query);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(movies.Count);
        result.Value.ShouldAllBe(x => x.MediaType == PlexMediaType.Movie);
    }

    [Test]
    public async Task ShouldNotDuplicateEpisodes_WhenSameEpisodeIsRequestedTwice()
    {
        // Arrange
        await SetupDatabase(
            66778,
            config =>
            {
                config.PlexTvShowLibraryCount = 1;
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 3;
            }
        );
        var episode = (await IDbContext.PlexTvShowEpisodes.ToListAsync(CancellationToken)).First();
        var query = new GetDownloadPreviewQuery(
            [
                new DownloadMediaDTO
                {
                    Type = PlexMediaType.Episode,
                    PlexServerId = episode.PlexServerId,
                    PlexLibraryId = episode.PlexLibraryId,
                    MediaIds = [episode.Id],
                    Qualities = [],
                },
                new DownloadMediaDTO
                {
                    Type = PlexMediaType.Episode,
                    PlexServerId = episode.PlexServerId,
                    PlexLibraryId = episode.PlexLibraryId,
                    MediaIds = [episode.Id],
                    Qualities = [],
                },
            ]
        );

        // Act
        var result = await TestHandlerExecuteAsync<List<DownloadPreview>>(query);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);
        result.Value.Single().Children.Count.ShouldBe(1);
        result.Value.Single().Children.Single().Children.Count.ShouldBe(1);
    }
}
