using PlexRipper.Domain;
using Riok.Mapperly.Abstractions;

namespace Data.Contracts;

[Mapper(UseReferenceHandling = true, RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class DownloadTaskGenericMapper
{
    public static DownloadTaskGeneric ToGeneric(this DownloadTaskMovie downloadTaskMovieFile)
    {
        var result = downloadTaskMovieFile.ToGenericMapper();
        result.Children ??= new List<DownloadTaskGeneric>();
        result.DownloadWorkerTasks ??= new List<DownloadWorkerTask>();
        result.DataReceived = result.Children.Sum(x => x.DataReceived);
        result.DataTotal = result.Children.Sum(x => x.DataTotal);
        result.DownloadSpeed = result.Children.Sum(x => x.DownloadSpeed);
        result.FileTransferSpeed = result.Children.Sum(x => x.FileTransferSpeed);
        result.Percentage = result.Children.Any() ? result.Children.Average(x => x.Percentage) : 0;
        return result;
    }

    private static partial DownloadTaskGeneric ToGenericMapper(this DownloadTaskMovie downloadTaskMovie);

    public static partial DownloadTaskGeneric ToGeneric(this DownloadTaskMovieFile downloadTaskMovieFile);

    #region TvShow

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskTvShow downloadTaskTvShow)
    {
        var result = downloadTaskTvShow.ToGenericMapper();
        result.DownloadWorkerTasks ??= new List<DownloadWorkerTask>();
        result.Children ??= new List<DownloadTaskGeneric>();
        result.Children = downloadTaskTvShow.Children.Select(x =>
            {
                x.Calculate();
                return x.ToGeneric();
            })
            .ToList();

        result.DataReceived = result.Children.Sum(x => x.DataReceived);
        result.DataTotal = result.Children.Sum(x => x.DataTotal);
        result.DownloadSpeed = result.Children.Sum(x => x.DownloadSpeed);
        result.FileTransferSpeed = result.Children.Sum(x => x.FileTransferSpeed);
        result.Percentage = result.Children.Any() ? result.Children.Average(x => x.Percentage) : 0;
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
        result.Children = downloadTaskTvShowSeason.Children.Select(x =>
            {
                x.Calculate();
                return x.ToGeneric();
            })
            .ToList();

        result.DataReceived = result.Children.Sum(x => x.DataReceived);
        result.DataTotal = result.Children.Sum(x => x.DataTotal);
        result.DownloadSpeed = result.Children.Sum(x => x.DownloadSpeed);
        result.FileTransferSpeed = result.Children.Sum(x => x.FileTransferSpeed);
        result.Percentage = result.Children.Any() ? result.Children.Average(x => x.Percentage) : 0;
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
        result.DataReceived = result.Children.Sum(x => x.DataReceived);
        result.DataTotal = result.Children.Sum(x => x.DataTotal);
        result.DownloadSpeed = result.Children.Sum(x => x.DownloadSpeed);
        result.FileTransferSpeed = result.Children.Sum(x => x.FileTransferSpeed);
        result.Percentage = result.Children.Any() ? result.Children.Average(x => x.Percentage) : 0;
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