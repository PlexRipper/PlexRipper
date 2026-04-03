using Reaparr.Domain.Validators;

namespace Reaparr.Application.UnitTests;

public class GenerateDownloadTaskMoviesCommandHandlerUnitTests : BaseUnitTest<GenerateDownloadTaskMoviesCommandHandler>
{
    private readonly DownloadTaskMovieValidator _validator = new();

    [Test]
    public void GenerateDownloadTaskMoviesCommandValidator_ShouldRejectNullRequest()
    {
        // Arrange
        var validator = new GenerateDownloadTaskMoviesCommandValidator();
        var command = new GenerateDownloadTaskMoviesCommand((CreateDownloadTasksRequest)null!);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(GenerateDownloadTaskMoviesCommand.Request));
    }

    [Test]
    public async Task ShouldHaveInsertedValidDownloadTaskMoviesInDatabase_WhenGivenValidPlexMovies()
    {
        // Arrange
        await SetupDatabase(
            18022,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 5;
            }
        );

        var plexMovies = await IDbContext.PlexMovies.ToListAsync(CancellationToken);
        var movies = new List<DownloadMediaDTO>
        {
            new()
            {
                Type = PlexMediaType.Movie,
                MediaIds = plexMovies.Select(x => x.Id).ToList(),
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
        };

        // Act
        var command = new GenerateDownloadTaskMoviesCommand(movies);
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var plexDownloadTaskMovies = await IDbContext.DownloadTaskMovie.IncludeAll().ToListAsync(CancellationToken);

        plexDownloadTaskMovies.Count.ShouldBe(5);

        foreach (var downloadTaskMovie in plexDownloadTaskMovies)
        {
            downloadTaskMovie.Calculate();
            var validationResult = await _validator.ValidateAsync(downloadTaskMovie, CancellationToken);

            // Ignore DownloadDirectory and DestinationDirectory errors as these are set in the DownloadJob
            var validErrors = validationResult.Errors.FindAll(x =>
                !x.PropertyName.Contains(nameof(DownloadTaskFileBase.DownloadDirectory))
                && !x.PropertyName.Contains(nameof(DownloadTaskFileBase.DestinationDirectory))
            );
            validErrors.ShouldBeEmpty();
        }
    }

    [Test]
    public async Task ShouldHaveDestinationFolderPathIdSet_WhenRequestContainsTheDestinationFolderPathIdSet()
    {
        // Arrange
        await SetupDatabase(
            18022,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 5;
            }
        );

        var plexMovies = await IDbContext.PlexMovies.ToListAsync(CancellationToken);
        var movies = new List<DownloadMediaDTO>
        {
            new()
            {
                Type = PlexMediaType.Movie,
                MediaIds = plexMovies.Select(x => x.Id).ToList(),
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
        };

        // Act
        var request = new CreateDownloadTasksRequest(movies, 99);
        var command = new GenerateDownloadTaskMoviesCommand(request);
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var plexDownloadTaskMovies = await IDbContext.DownloadTaskMovie.IncludeAll().ToListAsync(CancellationToken);

        plexDownloadTaskMovies.Count.ShouldBe(5);

        foreach (var downloadTaskMovie in plexDownloadTaskMovies)
        {
            foreach (var child in downloadTaskMovie.Children)
            {
                child.DestinationFolderPathId.ShouldBe(99);
            }
        }
    }

    [Test]
    public async Task ShouldHaveMultipleDownloadTaskMovieFile_WhenPlexMovieHasMultiParts()
    {
        // Arrange
        await SetupDatabase(
            9999,
            config =>
            {
                config.MovieCount = 2;
                config.IncludeMultiPartMovies = true;
            }
        );

        var plexMovies = await IDbContext.PlexMovies.ToListAsync(CancellationToken);
        var movies = new List<DownloadMediaDTO>
        {
            new()
            {
                Type = PlexMediaType.Movie,
                MediaIds = plexMovies.Select(x => x.Id).ToList(),
                PlexServerId = 1,
                PlexLibraryId = 1,
                Qualities = [],
            },
        };

        // Act
        var command = new GenerateDownloadTaskMoviesCommand(movies);
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var plexDownloadTaskMovies = await IDbContext
            .DownloadTaskMovie.IncludeAll()
            .Include(x => x.Children)
            .ToListAsync(CancellationToken);

        plexDownloadTaskMovies.Count.ShouldBe(2);

        foreach (var downloadTaskMovie in plexDownloadTaskMovies)
        {
            downloadTaskMovie.Calculate();
            var validationResult = await _validator.ValidateAsync(downloadTaskMovie, CancellationToken);

            // Ignore DownloadDirectory and DestinationDirectory errors as these are set in the DownloadJob
            var validErrors = validationResult.Errors.FindAll(x =>
                !x.PropertyName.Contains(nameof(DownloadTaskFileBase.DownloadDirectory))
                && !x.PropertyName.Contains(nameof(DownloadTaskFileBase.DestinationDirectory))
            );
            validErrors.ShouldBeEmpty();

            downloadTaskMovie.Children.Count.ShouldBe(2);
        }
    }

    [Test]
    public async Task ShouldKeepEachMovieBoundToItsOriginalLibrary_WhenGeneratingAcrossMultipleLibraries()
    {
        // Arrange
        await SetupDatabase(
            21123,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 2;
                config.MovieCount = 6;
            }
        );

        var moviesByLibrary = await IDbContext
            .PlexMovies.GroupBy(x => x.PlexLibraryId)
            .Select(x => new { PlexLibraryId = x.Key, MovieId = x.Select(y => y.Id).First() })
            .ToListAsync(CancellationToken);

        moviesByLibrary.Count.ShouldBeGreaterThanOrEqualTo(2);

        var movies = moviesByLibrary
            .Take(2)
            .Select(x => new DownloadMediaDTO
            {
                Type = PlexMediaType.Movie,
                MediaIds = [x.MovieId],
                PlexServerId = 1,
                PlexLibraryId = x.PlexLibraryId,
                Qualities = [],
            })
            .ToList();

        // Act
        var command = new GenerateDownloadTaskMoviesCommand(movies);
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var downloadTaskMovies = await IDbContext.DownloadTaskMovie.ToListAsync(CancellationToken);

        downloadTaskMovies.Count.ShouldBe(2);
        downloadTaskMovies.Select(x => x.PlexLibraryId).Distinct().Count().ShouldBe(2);
    }
}
