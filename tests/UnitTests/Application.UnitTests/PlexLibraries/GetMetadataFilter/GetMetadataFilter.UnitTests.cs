namespace Reaparr.Application.UnitTests;

public class GetMetadataFilterUnitTests
    : BaseEndpointUnitTest<GetMetadataFilter, GetMetadataFilterRequest, ResultDTO<PlexMediaFilterMetadataDTO>>
{
    [Test]
    [Arguments(PlexMediaType.Movie, false)]
    [Arguments(PlexMediaType.Movie, true)]
    [Arguments(PlexMediaType.OtherVideos, false)]
    [Arguments(PlexMediaType.OtherVideos, true)]
    public async Task ShouldReturnOnlyScopedDistinctQualities_WhenBrowsingMetadata(
        PlexMediaType mediaType,
        bool allLibraries
    )
    {
        // Arrange
        await SetupDatabase(8238, cfg =>
        {
            cfg.PlexMovieLibraryCount = 2;
            cfg.PlexOtherVideoLibraryCount = 2;
            cfg.MovieCount = 2;
            cfg.OtherVideoCount = 2;
        });
        var dbContext = IDbContext;
        var libraryIds = await dbContext.PlexLibraries.Where(x => x.Type == mediaType)
            .OrderBy(x => x.Id).Select(x => x.Id).ToListAsync(CancellationToken);
        libraryIds.Count.ShouldBe(2);
        var targetId = libraryIds[0];
        await dbContext.PlexMovieData.ExecuteUpdateAsync(x => x.SetProperty(y => y.Quality,
            y => mediaType == PlexMediaType.Movie
                ? (y.PlexLibraryId == targetId ? VideoQuality.FullHD : VideoQuality.UHD_4K)
                : VideoQuality.SD), CancellationToken);
        await dbContext.PlexOtherVideoData.ExecuteUpdateAsync(x => x.SetProperty(y => y.Quality,
            y => mediaType == PlexMediaType.OtherVideos
                ? (y.PlexLibraryId == targetId ? VideoQuality.FullHD : VideoQuality.UHD_4K)
                : VideoQuality.SD), CancellationToken);
        var request = new GetMetadataFilterRequest
        {
            PlexLibraryId = allLibraries ? 0 : targetId,
            MediaType = mediaType,
        };

        // Act
        var endpointResult = await TestEndpointHandleAsync(request);

        // Assert
        endpointResult.IsValid.ShouldBeTrue();
        endpointResult.StatusCode.ShouldBe(200);
        var response = endpointResult.Response.ShouldNotBeNull();
        response.IsSuccess.ShouldBeTrue();
        response.Errors.Count.ShouldBe(0);
        response.Value.ShouldNotBeNull().Qualities.Order().ShouldBe(
            allLibraries
                ? new[] { VideoQuality.FullHD.ToId(), VideoQuality.UHD_4K.ToId() }.Order()
                : new[] { VideoQuality.FullHD.ToId() });
    }

    [Test]
    public async Task ShouldKeepAudioAndVideoGenreIdsScoped_WhenNamesAndIdsOverlap()
    {
        // Arrange
        await SetupDatabase(8239, cfg =>
        {
            cfg.PlexMovieLibraryCount = 1;
            cfg.PlexMusicLibraryCount = 1;
        });
        var dbContext = IDbContext;
        var videoLibrary = await dbContext.PlexLibraries.SingleAsync(x => x.Type == PlexMediaType.Movie, CancellationToken);
        var audioLibrary = await dbContext.PlexLibraries.SingleAsync(x => x.Type == PlexMediaType.MusicArtist, CancellationToken);
        var videoGenre = new PlexGenreVideo { Name = "Shared", Key = "shared", Type = PlexGenreType.Drama };
        var audioGenre = new PlexGenreAudio { Name = "Shared", Key = "shared" };
        dbContext.PlexGenresVideo.Add(videoGenre);
        dbContext.PlexGenresAudio.Add(audioGenre);
        await dbContext.SaveChangesAsync(CancellationToken);
        dbContext.PlexLibraryGenresVideo.Add(new PlexLibraryGenresVideo(videoLibrary.Id, videoGenre.Id));
        dbContext.PlexLibraryGenresAudio.Add(new PlexLibraryGenresAudio(audioLibrary.Id, audioGenre.Id));
        await dbContext.SaveChangesAsync(CancellationToken);

        // Act
        var videoResult = await TestEndpointHandleAsync(
            new GetMetadataFilterRequest { PlexLibraryId = videoLibrary.Id, MediaType = PlexMediaType.Movie }
        );
        var audioResult = await TestEndpointHandleAsync(
            new GetMetadataFilterRequest { PlexLibraryId = audioLibrary.Id, MediaType = PlexMediaType.MusicArtist }
        );

        // Assert
        videoResult.IsValid.ShouldBeTrue();
        videoResult.Response.ShouldNotBeNull().Value.ShouldNotBeNull().Genres.ShouldBe([videoGenre.Id]);
        audioResult.IsValid.ShouldBeTrue();
        audioResult.Response.ShouldNotBeNull().Value.ShouldNotBeNull().Genres.ShouldBe([audioGenre.Id]);
        videoGenre.Id.ShouldBe(audioGenre.Id);
    }
}
