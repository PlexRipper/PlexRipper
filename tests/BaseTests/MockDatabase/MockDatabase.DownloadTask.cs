using Reaparr.Data;
using Reaparr.Data.Contracts;

namespace Reaparr.BaseTests;

public static partial class MockDatabase
{
    private static async Task<ReaparrDbContext> AddDownloadTaskMovies(
        this ReaparrDbContext context,
        Seed seed,
        Action<FakeDataConfig>? options = null
    )
    {
        var config = FakeDataConfig.FromOptions(options);
        var downloadTasks = FakeData.GetMovieDownloadTask(seed, options).Generate(config.MovieDownloadTasksCount);

        var plexLibrary = context.PlexLibraries.FirstOrDefault(x => x.Type == PlexMediaType.Movie);
        plexLibrary.ShouldNotBeNull(
            "No PlexLibrary available with type Movie, consider setting config.DisableForeignKeyCheck = true"
        );

        var plexServer = context.PlexServers.IncludeConnections().FirstOrDefault(x => x.Id == plexLibrary.PlexServerId);
        plexServer.ShouldNotBeNull();

        downloadTasks.SetRelationshipIds(plexLibrary.PlexServerId, plexLibrary.Id);

        context.DownloadTaskMovie.AddRange(downloadTasks);
        await context.SaveChangesAsync();

        _log.Here()
            .Debug(
                "Added {MovieDownloadTasksCount} Movie {NameOfDownloadTask}s to ReaparrDbContext: {DatabaseName}",
                config.MovieDownloadTasksCount,
                nameof(DownloadTaskMovie),
                context.DatabaseName
            );

        return context;
    }

    private static async Task<ReaparrDbContext> AddDownloadTaskTvShows(
        this ReaparrDbContext context,
        Seed seed,
        Action<FakeDataConfig>? options = null
    )
    {
        var config = FakeDataConfig.FromOptions(options);
        var downloadTasks = FakeData.GetDownloadTaskTvShow(seed, options).Generate(config.TvShowDownloadTasksCount);

        var plexLibrary = context.PlexLibraries.FirstOrDefault(x => x.Type == PlexMediaType.TvShow);
        plexLibrary.ShouldNotBeNull(
            "No PlexLibrary available with type TvShow, consider setting config.DisableForeignKeyCheck = true"
        );

        var plexServer = context.PlexServers.IncludeConnections().FirstOrDefault(x => x.Id == plexLibrary.PlexServerId);
        plexServer.ShouldNotBeNull();

        downloadTasks.SetRelationshipIds(plexLibrary.PlexServerId, plexLibrary.Id);

        context.DownloadTaskTvShow.AddRange(downloadTasks);
        await context.SaveChangesAsync();

        _log.Here()
            .Debug(
                "Added {TvShowDownloadTasksCount} TvShow {NameOfDownloadTask}s to ReaparrDbContext: {DatabaseName}",
                config.TvShowDownloadTasksCount,
                nameof(DownloadTaskTvShow),
                context.DatabaseName
            );

        return context;
    }
}
