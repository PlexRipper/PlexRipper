using Data.Contracts;
using PlexRipper.Data;

namespace PlexRipper.BaseTests;

public static partial class MockDatabase
{
    private static async Task<PlexRipperDbContext> AddDownloadTaskMovies(this PlexRipperDbContext context, Action<FakeDataConfig> options = null)
    {
        var config = FakeDataConfig.FromOptions(options);
        var downloadTasks = FakeData.GetMovieDownloadTaskV2(_seed, options).Generate(config.MovieDownloadTasksCount);

        if (!config.DisableForeignKeyCheck)
        {
            var plexLibrary = context.PlexLibraries.FirstOrDefault(x => x.Type == PlexMediaType.Movie);
            plexLibrary.ShouldNotBeNull("No PlexLibrary available with type Movie, consider setting config.DisableForeignKeyCheck = true");

            var plexServer = context.PlexServers.IncludeConnections().FirstOrDefault(x => x.Id == plexLibrary.PlexServerId);
            plexServer.ShouldNotBeNull();

            downloadTasks.SetRelationshipIds(plexLibrary.PlexServerId, plexLibrary.Id);
        }

        context.DownloadTaskMovie.AddRange(downloadTasks);
        await context.SaveChangesAsync();

        _log.Here()
            .Debug("Added {MovieDownloadTasksCount} Movie {NameOfDownloadTask}s to PlexRipperDbContext: {DatabaseName}", config.MovieDownloadTasksCount,
                nameof(DownloadTaskMovie), context.DatabaseName);

        return context;
    }

    private static async Task<PlexRipperDbContext> AddDownloadTaskTvShows(this PlexRipperDbContext context, Action<FakeDataConfig> options = null)
    {
        var config = FakeDataConfig.FromOptions(options);
        var downloadTasks = FakeData.GetDownloadTaskTvShow(_seed, options).Generate(config.TvShowDownloadTasksCount);

        if (!config.DisableForeignKeyCheck)
        {
            var plexLibrary = context.PlexLibraries.FirstOrDefault(x => x.Type == PlexMediaType.TvShow);
            plexLibrary.ShouldNotBeNull("No PlexLibrary available with type TvShow, consider setting config.DisableForeignKeyCheck = true");

            var plexServer = context.PlexServers.IncludeConnections().FirstOrDefault(x => x.Id == plexLibrary.PlexServerId);
            plexServer.ShouldNotBeNull();

            downloadTasks.SetRelationshipIds(plexLibrary.PlexServerId, plexLibrary.Id);
        }

        context.DownloadTaskTvShow.AddRange(downloadTasks);
        await context.SaveChangesAsync();

        _log.Here()
            .Debug("Added {TvShowDownloadTasksCount} TvShow {NameOfDownloadTask}s to PlexRipperDbContext: {DatabaseName}", config.TvShowDownloadTasksCount,
                nameof(DownloadTaskTvShow), context.DatabaseName);

        return context;
    }
}