using PlexRipper.Domain;
using Riok.Mapperly.Abstractions;

namespace Data.Contracts;

[Mapper(UseReferenceHandling = true, RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class DownloadTaskGenericMapper
{
    #region Movie

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskMovie downloadTaskMovieFile)
    {
        var result = downloadTaskMovieFile.ToGenericMapper();
        result.Children ??= new List<DownloadTaskGeneric>();
        result.DownloadWorkerTasks ??= new List<DownloadWorkerTask>();
        result.Calculate();
        return result;
    }

    private static partial DownloadTaskGeneric ToGenericMapper(this DownloadTaskMovie downloadTaskMovie);

    #endregion

    #region MovieFile

    public static partial DownloadTaskGeneric ToGeneric(this DownloadTaskMovieFile downloadTaskMovieFile);

    #endregion

    #region TvShow

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskTvShow downloadTaskTvShow)
    {
        var result = downloadTaskTvShow.ToGenericMapper();
        result.DownloadWorkerTasks ??= new List<DownloadWorkerTask>();
        result.Children ??= new List<DownloadTaskGeneric>();
        result.Calculate();
        return result;
    }

    private static partial DownloadTaskGeneric ToGenericMapper(this DownloadTaskTvShow downloadTaskTvShow);

    #endregion

    #region Season

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskTvShowSeason downloadTaskTvShowSeason)
    {
        var result = downloadTaskTvShowSeason.ToGenericMapper();
        result.DownloadWorkerTasks ??= new List<DownloadWorkerTask>();
        result.Children ??= new List<DownloadTaskGeneric>();
        result.Calculate();
        return result;
    }

    private static partial DownloadTaskGeneric ToGenericMapper(this DownloadTaskTvShowSeason downloadTaskTvShowSeason);

    #endregion

    #region Episode

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskTvShowEpisode downloadTaskTvShowEpisode)
    {
        var result = downloadTaskTvShowEpisode.ToGenericMapper();
        result.Children ??= new List<DownloadTaskGeneric>();
        result.DownloadWorkerTasks ??= new List<DownloadWorkerTask>();
        result.Calculate();
        return result;
    }

    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.DestinationDirectory))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.DownloadDirectory))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.FileLocationUrl))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.FileName))]
    private static partial DownloadTaskGeneric ToGenericMapper(this DownloadTaskTvShowEpisode downloadTaskTvShowEpisode);

    #endregion

    #region EpisodeFile

    public static partial DownloadTaskGeneric ToGeneric(this DownloadTaskTvShowEpisodeFile downloadTaskTvShowEpisodeFile);

    #endregion
}