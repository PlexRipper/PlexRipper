using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbSetExtensions
{
    public static IQueryable<DownloadTaskMovie> IncludeAll(this IQueryable<DownloadTaskMovie> downloadTasks) => downloadTasks
        .Include(x => x.PlexServer)
        .Include(x => x.PlexLibrary)
        .Include(x => x.Children)
        .ThenInclude(x => x.PlexServer)
        .Include(x => x.Children)
        .ThenInclude(x => x.PlexLibrary);

    public static IQueryable<DownloadTaskTvShow> IncludeAll(this IQueryable<DownloadTaskTvShow> downloadTasks) => downloadTasks
        .Include($"{nameof(DownloadTaskTvShow.PlexServer)}")
        .Include($"{nameof(DownloadTaskTvShow.PlexLibrary)}")
        .Include($"{nameof(DownloadTaskTvShow.Children)}.{nameof(DownloadTaskTvShowSeason.PlexServer)}")
        .Include($"{nameof(DownloadTaskTvShow.Children)}.{nameof(DownloadTaskTvShowSeason.PlexLibrary)}")
        .Include($"{nameof(DownloadTaskTvShow.Children)}.{nameof(DownloadTaskTvShowSeason.Children)}.{nameof(DownloadTaskTvShowEpisode.PlexLibrary)}")
        .Include($"{nameof(DownloadTaskTvShow.Children)}.{nameof(DownloadTaskTvShowSeason.Children)}.{nameof(DownloadTaskTvShowEpisode.PlexServer)}")
        .Include(
            $"{nameof(DownloadTaskTvShow.Children)}.{nameof(DownloadTaskTvShowSeason.Children)}.{nameof(DownloadTaskTvShowEpisode.Children)}.{nameof(DownloadTaskTvShowEpisodeFile.PlexLibrary)}")
        .Include(
            $"{nameof(DownloadTaskTvShow.Children)}.{nameof(DownloadTaskTvShowSeason.Children)}.{nameof(DownloadTaskTvShowEpisode.Children)}.{nameof(DownloadTaskTvShowEpisodeFile.PlexServer)}");

    public static IQueryable<DownloadTaskTvShowSeason> IncludeAll(this IQueryable<DownloadTaskTvShowSeason> downloadTasks) => downloadTasks
        .Include($"{nameof(DownloadTaskTvShowSeason.PlexServer)}")
        .Include($"{nameof(DownloadTaskTvShowSeason.PlexLibrary)}")
        .Include($"{nameof(DownloadTaskTvShowSeason.Children)}.{nameof(DownloadTaskTvShowEpisode.PlexServer)}")
        .Include($"{nameof(DownloadTaskTvShowSeason.Children)}.{nameof(DownloadTaskTvShowEpisode.PlexLibrary)}")
        .Include(
            $"{nameof(DownloadTaskTvShowSeason.Children)}.{nameof(DownloadTaskTvShowEpisode.Children)}.{nameof(DownloadTaskTvShowEpisodeFile.PlexLibrary)}")
        .Include(
            $"{nameof(DownloadTaskTvShowSeason.Children)}.{nameof(DownloadTaskTvShowEpisode.Children)}.{nameof(DownloadTaskTvShowEpisodeFile.PlexServer)}");

    public static IQueryable<DownloadTaskTvShowEpisode> IncludeAll(this IQueryable<DownloadTaskTvShowEpisode> downloadTasks) => downloadTasks
        .Include($"{nameof(DownloadTaskTvShowEpisode.PlexServer)}")
        .Include($"{nameof(DownloadTaskTvShowEpisode.PlexLibrary)}")
        .Include($"{nameof(DownloadTaskTvShowEpisode.Children)}.{nameof(DownloadTaskTvShowEpisodeFile.PlexServer)}")
        .Include($"{nameof(DownloadTaskTvShowEpisode.Children)}.{nameof(DownloadTaskTvShowEpisodeFile.PlexLibrary)}");
}