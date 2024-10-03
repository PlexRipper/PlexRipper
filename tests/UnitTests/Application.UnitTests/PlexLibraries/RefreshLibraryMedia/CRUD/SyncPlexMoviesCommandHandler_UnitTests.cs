namespace PlexRipper.Application.UnitTests;

public class SyncPlexMoviesCommandHandler_UnitTests : BaseUnitTest<SyncPlexMoviesCommandHandler>
{
    private SyncPlexMoviesCommandValidator _validator =
        new(LogManager.CreateLogInstance<SyncPlexMoviesCommandValidator>());

    public SyncPlexMoviesCommandHandler_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldCreateAllMovies_WhenNoneExistsYet()
    {
        // Arrange
        Seed = 451253;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
        });

        var library = IDbContext.PlexLibraries.First();
        library.ShouldNotBeNull();

        var movies = FakeData.GetPlexMovies(Seed).Generate(50);
        SetIds(library, movies);

        library.Movies = movies;

        // Act
        var request = new SyncPlexMoviesCommand(movies);
        (await _validator.ValidateAsync(request)).IsValid.ShouldBeTrue();
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var plexMoviesDb = IDbContext.PlexMovies.ToList();
        plexMoviesDb.Count.ShouldBe(50);
    }

    [Fact]
    public async Task ShouldDeleteTwentyMovies_WhenSomeExist()
    {
        // Arrange
        Seed = 4503253;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.PlexAccountCount = 1;
            config.MovieCount = 50;
        });

        var library = IDbContext.PlexLibraries.First();
        library.ShouldNotBeNull();
        var moviesDb = IDbContext.PlexMovies.ToList();
        moviesDb.ShouldNotBeNull();

        var movies = moviesDb.GetRange(0, 30);
        SetIds(library, movies);

        library.Movies = movies;

        // Act
        var request = new SyncPlexMoviesCommand(movies);
        (await _validator.ValidateAsync(request)).IsValid.ShouldBeTrue();
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var plexMoviesDb = IDbContext.PlexMovies.ToList();
        plexMoviesDb.Count.ShouldBe(30);
        foreach (var plexMovie in library.Movies)
            plexMoviesDb.Find(x => x.Key == plexMovie.Key).ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldCreateUpdateAndDeleteMovies_WhenSomeExist()
    {
        // Arrange
        Seed = 4903259;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.PlexAccountCount = 1;
            config.MovieCount = 50;
        });

        var library = IDbContext.PlexLibraries.First();
        var moviesDb = IDbContext.PlexMovies.ToList();
        library.ShouldNotBeNull();
        var newMovies = new List<PlexMovie>();
        newMovies.AddRange(moviesDb.GetRange(0, 10));
        newMovies.AddRange(FakeData.GetPlexMovies(45845).Generate(30));

        for (var i = 0; i < newMovies.Count; i++)
        {
            // Create updated movies based on the Key
            if (i is >= 10 and < 30)
            {
                newMovies[i].Key = moviesDb[i].Key;
                newMovies[i].UpdatedAt = DateTime.Now;
            }

            newMovies[i].Id = 0;
            newMovies[i].Title = $"TEST - {newMovies[i].Title}";
        }

        SetIds(library, newMovies);

        library.Movies = newMovies;

        // Act
        var request = new SyncPlexMoviesCommand(newMovies);
        (await _validator.ValidateAsync(request)).IsValid.ShouldBeTrue();
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var plexMoviesDb = IDbContext.PlexMovies.ToList();
        plexMoviesDb.Count.ShouldBe(40);
        foreach (var plexMovie in newMovies)
        {
            var findResult = plexMoviesDb.Find(x => x.Key == plexMovie.Key);
            findResult.ShouldNotBeNull();
            findResult.Title.Contains("TEST").ShouldBeTrue();
        }
    }

    private void SetIds(PlexLibrary library, List<PlexMovie> newPlexMovies)
    {
        foreach (var plexMovie in newPlexMovies)
        {
            plexMovie.PlexLibraryId = library.Id;
            plexMovie.PlexServerId = library.PlexServerId;
        }
    }
}
