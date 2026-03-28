using Microsoft.EntityFrameworkCore;

namespace Reaparr.PublicAPI.UnitTests;

public class GetTorrentFilesEndpointUnitTests : BaseUnitTest<GetTorrentFilesEndpoint>
{
    [Test]
    public async Task ShouldReturnEmptyList_WhenHashMatchesNoFiles()
    {
        // Arrange
        await SetupDatabase(
            4412,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieCount = 1;
                config.MovieDownloadTasksCount = 1;
                config.TvShowCount = 1;
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var request = new GetTorrentFilesRequest { Hash = "missing-hash" };

        // Act
        var endpoint = SetupEndpointUnitTest<GetTorrentFilesEndpoint>();
        await endpoint.HandleAsync(request, CancellationToken);
        var response = endpoint.Response;

        // Assert
        endpoint.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status200OK);
        response.ShouldNotBeNull();
        response.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldReturnFileNameOnly_WhenDownloadRootPathIsEmpty()
    {
        // Arrange
        await SetupDatabase(
            4413,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieCount = 1;
                config.MovieDownloadTasksCount = 1;
                config.TvShowCount = 0;
            }
        );

        var dbContext = IDbContext;
        var hash = "movie-hash-empty-root";
        var movieFile = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);

        await dbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .ExecuteUpdateAsync(
                x =>
                    x.SetProperty(p => p.HashId, hash)
                        .SetProperty(p => p.FileName, "only-file-name.mkv")
                        .SetProperty(p => p.DirectoryMeta, BuildDirectoryMeta("", "Movie Folder", "", "")),
                CancellationToken
            );

        var request = new GetTorrentFilesRequest { Hash = hash };

        // Act
        var endpoint = SetupEndpointUnitTest<GetTorrentFilesEndpoint>();
        await endpoint.HandleAsync(request, CancellationToken);
        var response = endpoint.Response;

        // Assert
        endpoint.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status200OK);
        response.ShouldNotBeNull();
        response.Count.ShouldBe(1);
        response.Single().Name.ShouldBe("only-file-name.mkv");
    }

    [Test]
    public async Task ShouldReturnFileNameOnly_WhenComputedDirectoryEscapesDownloadRoot()
    {
        // Arrange
        await SetupDatabase(
            4414,
            config =>
            {
                config.PlexServerCount = 1;
                config.TvShowCount = 1;
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
                config.MovieCount = 0;
            }
        );

        var dbContext = IDbContext;
        var hash = "episode-hash-outside-root";
        var episodeFile = await dbContext.DownloadTaskTvShowEpisodeFile.FirstAsync(CancellationToken);

        await dbContext
            .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == episodeFile.Id)
            .ExecuteUpdateAsync(
                x =>
                    x.SetProperty(p => p.HashId, hash)
                        .SetProperty(p => p.FileName, "outside-root-file.mkv")
                        .SetProperty(
                            p => p.DirectoryMeta,
                            BuildDirectoryMeta("/downloads", "", "/outside-root", "Season 01")
                        ),
                CancellationToken
            );

        var request = new GetTorrentFilesRequest { Hash = hash };

        // Act
        var endpoint = SetupEndpointUnitTest<GetTorrentFilesEndpoint>();
        await endpoint.HandleAsync(request, CancellationToken);
        var response = endpoint.Response;

        // Assert
        endpoint.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status200OK);
        response.ShouldNotBeNull();
        response.Count.ShouldBe(1);
        response.Single().Name.ShouldBe("outside-root-file.mkv");
    }

    [Test]
    public void GetTorrentFilesRequestValidator_ShouldRequireHash()
    {
        // Arrange
        var validator = new GetTorrentFilesRequestValidator();

        // Act
        var result = validator.Validate(new GetTorrentFilesRequest { Hash = string.Empty });

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(GetTorrentFilesRequest.Hash));
    }

    [Test]
    public void GetTorrentFilesRequestValidator_ShouldPass_WhenHashIsProvided()
    {
        // Arrange
        var validator = new GetTorrentFilesRequestValidator();

        // Act
        var result = validator.Validate(new GetTorrentFilesRequest { Hash = "abc123" });

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    private static DownloadTaskDirectory BuildDirectoryMeta(
        string downloadRootPath,
        string movieFolder,
        string tvShowFolder,
        string seasonFolder
    ) =>
        new()
        {
            DestinationRootPath = "/destination",
            DownloadRootPath = downloadRootPath,
            MovieFolder = movieFolder,
            TvShowFolder = tvShowFolder,
            SeasonFolder = seasonFolder,
            KeepCompletedInDownloadFolder = false,
        };
}
