using PlexRipper.Application;

namespace Data.UnitTests;

public class CreateUpdateOrDeletePlexMoviesHandler_UnitTests : BaseUnitTest
{
    public CreateUpdateOrDeletePlexMoviesHandler_UnitTests(ITestOutputHelper output)
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
        library.Movies = FakeData.GetPlexMovies(Seed).Generate(50);

        // Act
        var request = new CreateUpdateOrDeletePlexMoviesCommand(library);
        var handler = new CreateUpdateOrDeletePlexMoviesHandler(Log, GetDbContext());
        var result = await handler.Handle(request, CancellationToken.None);

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

        library.Movies = moviesDb.GetRange(0, 30);

        // Act
        var request = new CreateUpdateOrDeletePlexMoviesCommand(library);
        var handler = new CreateUpdateOrDeletePlexMoviesHandler(Log, GetDbContext());
        var result = await handler.Handle(request, CancellationToken.None);

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

        library.Movies = newMovies;

        // Act
        var request = new CreateUpdateOrDeletePlexMoviesCommand(library);
        var handler = new CreateUpdateOrDeletePlexMoviesHandler(Log, GetDbContext());
        var result = await handler.Handle(request, CancellationToken.None);

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
}
