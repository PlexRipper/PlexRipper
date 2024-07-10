using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DownloadTaskKeyMapper
{
    #region PlexMovie

    public static IQueryable<DownloadTaskKey> ProjectToKey(this IQueryable<DownloadTaskMovie> downloadTaskMovie) => downloadTaskMovie.Select(x =>
        new DownloadTaskKey
        {
            Id = x.Id,
            PlexServerId = x.PlexServerId,
            PlexLibraryId = x.PlexLibraryId,
            Type = x.DownloadTaskType,
        });

    #endregion

    #region PlexMovieFile

    public static IQueryable<DownloadTaskKey> ProjectToKey(this IQueryable<DownloadTaskMovieFile> downloadTaskMovieFile) => downloadTaskMovieFile.Select(x =>
        new DownloadTaskKey
        {
            Id = x.Id,
            PlexServerId = x.PlexServerId,
            PlexLibraryId = x.PlexLibraryId,
            Type = x.DownloadTaskType,
        });

    public static IQueryable<DownloadTaskKey> ProjectToParentKey(this IQueryable<DownloadTaskMovieFile> downloadTaskMovieFile) => downloadTaskMovieFile.Select(
        x => new DownloadTaskKey
        {
            Id = x.ParentId,
            PlexServerId = x.PlexServerId,
            PlexLibraryId = x.PlexLibraryId,
            Type = DownloadTaskType.Movie,
        });

    #endregion

    #region PlexTvShow

    public static IQueryable<DownloadTaskKey> ProjectToKey(this IQueryable<DownloadTaskTvShow> downloadTaskTvShow) => downloadTaskTvShow.Select(x =>
        new DownloadTaskKey
        {
            Id = x.Id,
            PlexServerId = x.PlexServerId,
            PlexLibraryId = x.PlexLibraryId,
            Type = x.DownloadTaskType,
        });

    #endregion

    #region PlexSeason

    public static IQueryable<DownloadTaskKey> ProjectToKey(this IQueryable<DownloadTaskTvShowSeason> downloadTaskTvShowSeason) =>
        downloadTaskTvShowSeason.Select(x => new DownloadTaskKey
        {
            Id = x.Id,
            PlexServerId = x.PlexServerId,
            PlexLibraryId = x.PlexLibraryId,
            Type = x.DownloadTaskType,
        });

    #endregion

    #region PlexTvShowEpisode

    public static IQueryable<DownloadTaskKey> ProjectToKey(this IQueryable<DownloadTaskTvShowEpisode> downloadTaskTvShowEpisode) =>
        downloadTaskTvShowEpisode.Select(x => new DownloadTaskKey
        {
            Id = x.Id,
            PlexServerId = x.PlexServerId,
            PlexLibraryId = x.PlexLibraryId,
            Type = x.DownloadTaskType,
        });

    #endregion

    #region PlexTvShowEpisodeFile

    public static IQueryable<DownloadTaskKey> ProjectToKey(this IQueryable<DownloadTaskTvShowEpisodeFile> downloadTaskTvShowEpisodeFile) =>
        downloadTaskTvShowEpisodeFile.Select(x => new DownloadTaskKey
        {
            Id = x.Id,
            PlexServerId = x.PlexServerId,
            PlexLibraryId = x.PlexLibraryId,
            Type = x.DownloadTaskType,
        });

    public static IQueryable<DownloadTaskKey> ProjectToParentKey(this IQueryable<DownloadTaskTvShowEpisodeFile> downloadTaskTvShowEpisodeFile) =>
        downloadTaskTvShowEpisodeFile.Select(x => new DownloadTaskKey
        {
            Id = x.ParentId,
            PlexServerId = x.PlexServerId,
            PlexLibraryId = x.PlexLibraryId,
            Type = DownloadTaskType.Episode,
        });

    #endregion
}