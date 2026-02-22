using Microsoft.EntityFrameworkCore;
using Reaparr.BackgroundJobs.Contracts;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.PlexApi.UnitTests;

public class GetLibraryMediaFromPlexApiCommandHandlerUnitTests : BaseUnitTest<GetLibraryMediaFromPlexApiCommandHandler>
{
    public GetLibraryMediaFromPlexApiCommandHandlerUnitTests(ITestOutputHelper output)
        : base(output) { }

    private void SetupProgressStoreMocks()
    {
        Mock.Mock<ILibrarySyncProgressStore>()
            .Setup(x => x.StartAsync(It.IsAny<int>(), It.IsAny<PlexMediaType>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Mock.Mock<ILibrarySyncProgressStore>()
            .Setup(x =>
                x.UpdateItemAsync(It.IsAny<int>(), It.IsAny<LibraryProgressItem>(), It.IsAny<CancellationToken>())
            )
            .Returns(Task.CompletedTask);

        Mock.Mock<ILibrarySyncProgressStore>()
            .Setup(x => x.UpdateErrorAsync(It.IsAny<int>(), It.IsAny<Result>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private PlexLibrary BuildPlexLibrary(PlexLibrary dbLibrary, PlexMediaType type)
    {
        var seed = new Seed(9000);
        var fakeLibrary = FakeData.GetPlexLibrary(seed, type).Generate();
        return new PlexLibrary
        {
            Id = dbLibrary.Id,
            Key = dbLibrary.Key,
            Type = type,
            Title = fakeLibrary.Title,
            PlexServerId = dbLibrary.PlexServerId,
            Uuid = fakeLibrary.Uuid,
            Language = fakeLibrary.Language,
            CreatedAt = fakeLibrary.CreatedAt,
            UpdatedAt = fakeLibrary.UpdatedAt,
            ScannedAt = fakeLibrary.ScannedAt,
        };
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenGetLibrarySectionsFails()
    {
        // Arrange
        await SetupDatabase(2001, config => config.PlexMovieLibraryCount = 1);
        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync(CancellationToken);
        plexLibrary.ShouldNotBeNull();

        Mock.SetupCommand(It.IsAny<GetLibrarySectionsCommand>)
            .ReturnsAsync(Result.Fail<List<PlexLibrary>>("Connection refused"));

        SetupProgressStoreMocks();

        // Act
        var command = new GetLibraryMediaFromPlexApiCommand(plexLibrary);
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(x => x.Message.Contains("Connection refused"));

        Mock.Mock<ILibrarySyncProgressStore>()
            .Verify(
                x => x.StartAsync(It.IsAny<int>(), It.IsAny<PlexMediaType>(), It.IsAny<CancellationToken>()),
                Times.Never()
            );
        Mock.Mock<ILibrarySyncProgressStore>()
            .Verify(
                x => x.UpdateErrorAsync(It.IsAny<int>(), It.IsAny<Result>(), It.IsAny<CancellationToken>()),
                Times.Never()
            );
        Mock.Mock<ILibrarySyncProgressStore>()
            .Verify(
                x => x.UpdateItemAsync(It.IsAny<int>(), It.IsAny<LibraryProgressItem>(), It.IsAny<CancellationToken>()),
                Times.Never()
            );
    }

    [Fact]
    public async Task ShouldReturnNotFoundError_WhenLibraryKeyNoLongerExistsOnServer()
    {
        // Arrange
        await SetupDatabase(2002, config => config.PlexMovieLibraryCount = 1);
        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync(CancellationToken);
        plexLibrary.ShouldNotBeNull();

        // Return a list of libraries from the server that does not contain the library key
        var fakeLib = FakeData.GetPlexLibrary(new Seed(2002), PlexMediaType.Movie).Generate();
        var otherLibrary = new PlexLibrary
        {
            Key = "999999",
            Type = PlexMediaType.Movie,
            Title = fakeLib.Title,
            PlexServerId = fakeLib.PlexServerId,
            Uuid = fakeLib.Uuid,
            Language = fakeLib.Language,
            CreatedAt = fakeLib.CreatedAt,
            UpdatedAt = fakeLib.UpdatedAt,
            ScannedAt = fakeLib.ScannedAt,
        };

        Mock.SetupCommand(It.IsAny<GetLibrarySectionsCommand>)
            .ReturnsAsync(Result.Ok(new List<PlexLibrary> { otherLibrary }));

        SetupProgressStoreMocks();

        // Act
        var command = new GetLibraryMediaFromPlexApiCommand(plexLibrary);
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Has404NotFoundError().ShouldBeTrue();

        Mock.Mock<ILibrarySyncProgressStore>()
            .Verify(
                x => x.StartAsync(It.IsAny<int>(), It.IsAny<PlexMediaType>(), It.IsAny<CancellationToken>()),
                Times.Never()
            );
    }

    [Theory]
    [InlineData(PlexMediaType.Music)]
    [InlineData(PlexMediaType.Photos)]
    public async Task ShouldReturnEmptyLibraryMetadata_WhenLibraryTypeIsNotSupported(PlexMediaType libraryType)
    {
        // Arrange
        await SetupDatabase(2003, config => config.PlexMovieLibraryCount = 1);
        var dbLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync(CancellationToken);
        dbLibrary.ShouldNotBeNull();

        var updatedLibrary = new PlexLibrary
        {
            Id = dbLibrary.Id,
            Key = dbLibrary.Key,
            Type = libraryType,
            Title = "Unsupported Library",
            PlexServerId = dbLibrary.PlexServerId,
            Uuid = dbLibrary.Uuid,
            Language = dbLibrary.Language,
            CreatedAt = dbLibrary.CreatedAt,
            UpdatedAt = dbLibrary.UpdatedAt,
            ScannedAt = dbLibrary.ScannedAt,
        };

        Mock.SetupCommand(It.IsAny<GetLibrarySectionsCommand>)
            .ReturnsAsync(Result.Ok(new List<PlexLibrary> { updatedLibrary }));

        SetupProgressStoreMocks();

        // Act
        var command = new GetLibraryMediaFromPlexApiCommand(dbLibrary);
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Library.ShouldNotBeNull();
        result.Value.Library.Movies.ShouldBeEmpty();
        result.Value.Library.TvShows.ShouldBeEmpty();

        Mock.Mock<ILibrarySyncProgressStore>()
            .Verify(
                x => x.StartAsync(It.IsAny<int>(), It.IsAny<PlexMediaType>(), It.IsAny<CancellationToken>()),
                Times.Never()
            );
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<GetAllMediaByTypeFromPlexApiCommand>(), It.IsAny<CancellationToken>()),
                Times.Never()
            );
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenGetAllMediaByTypeFails()
    {
        // Arrange
        await SetupDatabase(2004, config => config.PlexMovieLibraryCount = 1);
        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync(CancellationToken);
        plexLibrary.ShouldNotBeNull();

        var updatedLibrary = BuildPlexLibrary(plexLibrary, PlexMediaType.Movie);

        Mock.SetupCommand(It.IsAny<GetLibrarySectionsCommand>)
            .ReturnsAsync(Result.Ok(new List<PlexLibrary> { updatedLibrary }));

        Mock.SetupCommand(It.IsAny<GetAllMediaByTypeFromPlexApiCommand>)
            .ReturnsAsync(Result.Fail<List<LibraryMediaItemDTO>>("API timeout"));

        SetupProgressStoreMocks();

        // Act
        var command = new GetLibraryMediaFromPlexApiCommand(plexLibrary);
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(x => x.Message.Contains("API timeout"));

        Mock.Mock<ILibrarySyncProgressStore>()
            .Verify(
                x => x.StartAsync(plexLibrary.Id, PlexMediaType.Movie, It.IsAny<CancellationToken>()),
                Times.Once()
            );
        Mock.Mock<ILibrarySyncProgressStore>()
            .Verify(
                x => x.UpdateErrorAsync(It.IsAny<int>(), It.IsAny<Result>(), It.IsAny<CancellationToken>()),
                Times.Never()
            );
        Mock.Mock<ILibrarySyncProgressStore>()
            .Verify(
                x => x.UpdateItemAsync(It.IsAny<int>(), It.IsAny<LibraryProgressItem>(), It.IsAny<CancellationToken>()),
                Times.Never()
            );
    }

    [Theory]
    [InlineData(PlexMediaType.Movie)]
    [InlineData(PlexMediaType.TvShow)]
    public async Task ShouldReturnOkResultWithPopulatedMetadata_WhenLibraryIsSynced(PlexMediaType libraryType)
    {
        // Arrange
        var seed = await SetupDatabase(
            2005,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = libraryType == PlexMediaType.Movie ? 1 : 0;
                config.PlexTvShowLibraryCount = libraryType == PlexMediaType.TvShow ? 1 : 0;
            }
        );

        var plexLibrary = await IDbContext
            .PlexLibraries.Where(x => x.Type == libraryType)
            .FirstOrDefaultAsync(CancellationToken);
        plexLibrary.ShouldNotBeNull();

        var updatedLibrary = BuildPlexLibrary(plexLibrary, libraryType);

        var mediaItems = FakeData.GetLibraryMediaItemDTO(seed, mediaType: libraryType).Generate(5);

        Mock.SetupCommand(It.IsAny<GetLibrarySectionsCommand>)
            .ReturnsAsync(Result.Ok(new List<PlexLibrary> { updatedLibrary }));

        Mock.SetupCommand(It.IsAny<GetAllMediaByTypeFromPlexApiCommand>).ReturnsAsync(Result.Ok(mediaItems));

        SetupProgressStoreMocks();

        // Act
        var command = new GetLibraryMediaFromPlexApiCommand(plexLibrary);
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        result.Value.Countries.ShouldNotBeEmpty();
        result.Value.Genres.ShouldNotBeEmpty();
        result.Value.Actors.ShouldNotBeEmpty();

        switch (libraryType)
        {
            case PlexMediaType.Movie:
                result.Value.Library.Movies.ShouldNotBeEmpty();
                result.Value.Library.TvShows.ShouldBeEmpty();
                break;
            case PlexMediaType.TvShow:
                result.Value.Library.TvShows.ShouldNotBeEmpty();
                result.Value.Library.Movies.ShouldBeEmpty();
                break;
        }

        Mock.Mock<ILibrarySyncProgressStore>()
            .Verify(x => x.StartAsync(plexLibrary.Id, libraryType, It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task ShouldPreserveLibraryIdAndServerId_WhenUpdatedLibraryIsReturned()
    {
        // Arrange
        await SetupDatabase(2006, config => config.PlexMovieLibraryCount = 1);
        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync(CancellationToken);
        plexLibrary.ShouldNotBeNull();

        var updatedLibrary = BuildPlexLibrary(plexLibrary, PlexMediaType.Movie);
        // Simulate server returning a library with different Id/PlexServerId (as the handler corrects these)
        updatedLibrary.Id = 0;
        updatedLibrary.PlexServerId = 0;

        var mediaItems = FakeData.GetLibraryMediaItemDTO(new Seed(2006), mediaType: PlexMediaType.Movie).Generate(3);

        Mock.SetupCommand(It.IsAny<GetLibrarySectionsCommand>)
            .ReturnsAsync(Result.Ok(new List<PlexLibrary> { updatedLibrary }));

        Mock.SetupCommand(It.IsAny<GetAllMediaByTypeFromPlexApiCommand>).ReturnsAsync(Result.Ok(mediaItems));

        SetupProgressStoreMocks();

        // Act
        var command = new GetLibraryMediaFromPlexApiCommand(plexLibrary);
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Library.Id.ShouldBe(plexLibrary.Id);
        result.Value.Library.PlexServerId.ShouldBe(plexLibrary.PlexServerId);

        Mock.Mock<ILibrarySyncProgressStore>()
            .Verify(
                x => x.StartAsync(plexLibrary.Id, PlexMediaType.Movie, It.IsAny<CancellationToken>()),
                Times.Once()
            );
        Mock.Mock<ILibrarySyncProgressStore>()
            .Verify(
                x => x.UpdateErrorAsync(It.IsAny<int>(), It.IsAny<Result>(), It.IsAny<CancellationToken>()),
                Times.Never()
            );
    }

    [Fact]
    public async Task ShouldReturnSortedMediaList_WhenMovieLibraryIsSynced()
    {
        // Arrange
        var seed = await SetupDatabase(2007, config => config.PlexMovieLibraryCount = 1);

        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync(CancellationToken);
        plexLibrary.ShouldNotBeNull();

        var updatedLibrary = BuildPlexLibrary(plexLibrary, PlexMediaType.Movie);

        // Generate media items with out-of-order titles to verify natural sort
        var mediaItems = FakeData.GetLibraryMediaItemDTO(seed, mediaType: PlexMediaType.Movie).Generate(10);

        Mock.SetupCommand(It.IsAny<GetLibrarySectionsCommand>)
            .ReturnsAsync(Result.Ok(new List<PlexLibrary> { updatedLibrary }));

        Mock.SetupCommand(It.IsAny<GetAllMediaByTypeFromPlexApiCommand>).ReturnsAsync(Result.Ok(mediaItems));

        SetupProgressStoreMocks();

        // Act
        var command = new GetLibraryMediaFromPlexApiCommand(plexLibrary);
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var movies = result.Value.Library.Movies.ToList();
        movies.Count.ShouldBe(mediaItems.Count);

        var expectedTitles = mediaItems.OrderByNatural(x => x.TitleSort).Select(x => x.Title).ToList();
        movies.Select(x => x.Title).ShouldBe(expectedTitles);

        Mock.Mock<ILibrarySyncProgressStore>()
            .Verify(
                x => x.StartAsync(plexLibrary.Id, PlexMediaType.Movie, It.IsAny<CancellationToken>()),
                Times.Once()
            );
        Mock.Mock<ILibrarySyncProgressStore>()
            .Verify(
                x => x.UpdateErrorAsync(It.IsAny<int>(), It.IsAny<Result>(), It.IsAny<CancellationToken>()),
                Times.Never()
            );
    }
}
