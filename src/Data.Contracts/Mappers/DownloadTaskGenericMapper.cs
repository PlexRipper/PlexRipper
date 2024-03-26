using PlexRipper.Domain;
using Riok.Mapperly.Abstractions;

namespace Data.Contracts;

[Mapper(UseReferenceHandling = true)]
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

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(DownloadTaskMovie.Children), nameof(DownloadTaskGeneric.Children))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.DestinationDirectory))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.DownloadDirectory))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.FileLocationUrl))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.FileName))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.DownloadWorkerTasks))]
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

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(DownloadTaskTvShow.Children), nameof(DownloadTaskGeneric.Children))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.DestinationDirectory))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.DownloadDirectory))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.FileLocationUrl))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.FileName))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.DownloadWorkerTasks))]
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

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(DownloadTaskTvShowSeason.Children), nameof(DownloadTaskGeneric.Children))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.DestinationDirectory))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.DownloadDirectory))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.FileLocationUrl))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.FileName))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.DownloadWorkerTasks))]
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

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(DownloadTaskTvShowEpisode.Children), nameof(DownloadTaskGeneric.Children))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.DestinationDirectory))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.DownloadDirectory))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.FileLocationUrl))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.FileName))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.DownloadWorkerTasks))]
    [MapperIgnoreSource(nameof(DownloadTaskTvShowEpisode.Parent))]
    [MapperIgnoreSource(nameof(DownloadTaskTvShowEpisode.Count))]
    [MapperIgnoreSource(nameof(DownloadTaskTvShowEpisode.Year))]
    private static partial DownloadTaskGeneric ToGenericMapper(this DownloadTaskTvShowEpisode downloadTaskTvShowEpisode);

    #endregion

    #region EpisodeFile

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(DownloadTaskTvShowEpisodeFile.DownloadWorkerTasks), nameof(DownloadTaskGeneric.DownloadWorkerTasks))]
    [MapperIgnoreTarget(nameof(DownloadTaskGeneric.Children))]
    [MapperIgnoreSource(nameof(DownloadTaskTvShowEpisodeFile.Parent))]
    [MapperIgnoreSource(nameof(DownloadTaskTvShowEpisodeFile.Count))]
    [MapperIgnoreSource(nameof(DownloadTaskTvShowEpisodeFile.Quality))]
    [MapperIgnoreSource(nameof(DownloadTaskTvShowEpisodeFile.GetFilePathsCompressed))]
    [MapperIgnoreSource(nameof(DownloadTaskTvShowEpisodeFile.DownloadSpeedFormatted))]
    public static partial DownloadTaskGeneric ToGeneric(this DownloadTaskTvShowEpisodeFile downloadTaskTvShowEpisodeFile);

    #endregion
}