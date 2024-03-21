using PlexRipper.Domain;
using Riok.Mapperly.Abstractions;

namespace Data.Contracts;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public static partial class DownloadTaskKeyMapper
{
    #region PlexMovie

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(DownloadTaskMovie.DownloadTaskType), nameof(DownloadTaskKey.Type))]
    public static partial IQueryable<DownloadTaskKey> ProjectToKey(this IQueryable<DownloadTaskMovie> downloadTaskMovie);

    #endregion

    #region PlexMovieFile

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(DownloadTaskMovieFile.DownloadTaskType), nameof(DownloadTaskKey.Type))]
    public static partial IQueryable<DownloadTaskKey> ProjectToKey(this IQueryable<DownloadTaskMovieFile> downloadTaskMovieFile);

    #endregion

    #region PlexTvShow

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(DownloadTaskTvShow.DownloadTaskType), nameof(DownloadTaskKey.Type))]
    public static partial IQueryable<DownloadTaskKey> ProjectToKey(this IQueryable<DownloadTaskTvShow> downloadTaskTvShow);

    #endregion

    #region PlexSeason

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(DownloadTaskTvShowSeason.DownloadTaskType), nameof(DownloadTaskKey.Type))]
    public static partial IQueryable<DownloadTaskKey> ProjectToKey(this IQueryable<DownloadTaskTvShowSeason> downloadTaskTvShowSeason);

    #endregion

    #region PlexTvShowEpisode

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(DownloadTaskTvShowEpisode.DownloadTaskType), nameof(DownloadTaskKey.Type))]
    public static partial IQueryable<DownloadTaskKey> ProjectToKey(this IQueryable<DownloadTaskTvShowEpisode> downloadTaskTvShowEpisode);

    #endregion

    #region PlexTvShowEpisodeFile

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(DownloadTaskTvShowEpisodeFile.DownloadTaskType), nameof(DownloadTaskKey.Type))]
    public static partial IQueryable<DownloadTaskKey> ProjectToKey(this IQueryable<DownloadTaskTvShowEpisodeFile> downloadTaskTvShowEpisodeFile);

    #endregion
}