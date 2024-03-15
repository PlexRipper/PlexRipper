namespace PlexRipper.Domain;

public static class DownloadTaskExtensions_Mappers
{
    public static DownloadTaskGeneric ToGeneric(this DownloadTaskMovie downloadTaskMovie)
    {
        if (downloadTaskMovie is null)
            throw new ArgumentNullException(nameof(downloadTaskMovie));

        var children = downloadTaskMovie.Children?.Select(x => x.ToGeneric())
            .ToList() ?? new List<DownloadTaskGeneric>();
        return new DownloadTaskGeneric
        {
            Children = children,
            CreatedAt = downloadTaskMovie.CreatedAt,
            DataReceived = children.Sum(x => x.DataReceived),
            DataTotal = children.Sum(x => x.DataTotal),
            DestinationDirectory = string.Empty,
            DownloadDirectory = string.Empty,
            DownloadSpeed = children.Sum(x => x.DownloadSpeed),
            DownloadStatus = downloadTaskMovie.DownloadStatus,
            DownloadTaskType = downloadTaskMovie.DownloadTaskType,
            DownloadWorkerTasks = new List<DownloadWorkerTask>(),
            FileLocationUrl = string.Empty,
            FileName = string.Empty,
            FileTransferSpeed = children.Sum(x => x.FileTransferSpeed),
            Title = downloadTaskMovie.Title,
            FullTitle = downloadTaskMovie.FullTitle,
            Id = downloadTaskMovie.Id,
            IsDownloadable = downloadTaskMovie.IsDownloadable,
            Key = downloadTaskMovie.Key,
            MediaType = downloadTaskMovie.MediaType,
            Percentage = children.Any() ? children.Average(x => x.Percentage) : 0,
            PlexLibrary = downloadTaskMovie.PlexLibrary,
            PlexLibraryId = downloadTaskMovie.PlexLibraryId,
            PlexServer = downloadTaskMovie.PlexServer,
            PlexServerId = downloadTaskMovie.PlexServerId,
        };
    }

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskMovieFile downloadTaskMovieFile) => new()
    {
        Children = new List<DownloadTaskGeneric>(),
        CreatedAt = downloadTaskMovieFile.CreatedAt,
        DataReceived = downloadTaskMovieFile.DataReceived,
        DataTotal = downloadTaskMovieFile.DataTotal,
        DestinationDirectory = downloadTaskMovieFile.DestinationDirectory,
        DownloadDirectory = downloadTaskMovieFile.DownloadDirectory,
        DownloadSpeed = downloadTaskMovieFile.DownloadSpeed,
        DownloadStatus = downloadTaskMovieFile.DownloadStatus,
        DownloadTaskType = downloadTaskMovieFile.DownloadTaskType,
        DownloadWorkerTasks = downloadTaskMovieFile.DownloadWorkerTasks,
        FileLocationUrl = downloadTaskMovieFile.FileLocationUrl,
        FileName = downloadTaskMovieFile.FileName,
        FileTransferSpeed = downloadTaskMovieFile.FileTransferSpeed,
        Title = downloadTaskMovieFile.Title,
        FullTitle = downloadTaskMovieFile.FullTitle,
        Id = downloadTaskMovieFile.Id,
        IsDownloadable = downloadTaskMovieFile.IsDownloadable,
        Key = downloadTaskMovieFile.Key,
        MediaType = downloadTaskMovieFile.MediaType,
        Percentage = downloadTaskMovieFile.Percentage,
        PlexLibrary = downloadTaskMovieFile.PlexLibrary,
        PlexLibraryId = downloadTaskMovieFile.PlexLibraryId,
        PlexServer = downloadTaskMovieFile.PlexServer,
        PlexServerId = downloadTaskMovieFile.PlexServerId,
    };

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskTvShow downloadTaskTvShow)
    {
        if (downloadTaskTvShow is null)
            throw new ArgumentNullException(nameof(downloadTaskTvShow));

        var children = downloadTaskTvShow.Children?.Select(x =>
            {
                x.Calculate();
                return x.ToGeneric();
            })
            .ToList() ?? new List<DownloadTaskGeneric>();

        return new DownloadTaskGeneric
        {
            Children = children,
            CreatedAt = downloadTaskTvShow.CreatedAt,
            DataReceived = children.Sum(x => x.DataReceived),
            DataTotal = children.Sum(x => x.DataTotal),
            DestinationDirectory = string.Empty,
            DownloadDirectory = string.Empty,
            DownloadSpeed = children.Sum(x => x.DownloadSpeed),
            DownloadStatus = downloadTaskTvShow.DownloadStatus,
            DownloadTaskType = downloadTaskTvShow.DownloadTaskType,
            DownloadWorkerTasks = new List<DownloadWorkerTask>(),
            FileLocationUrl = string.Empty,
            FileName = string.Empty,
            FileTransferSpeed = children.Sum(x => x.FileTransferSpeed),
            Title = downloadTaskTvShow.Title,
            FullTitle = downloadTaskTvShow.FullTitle,
            Id = downloadTaskTvShow.Id,
            IsDownloadable = downloadTaskTvShow.IsDownloadable,
            Key = downloadTaskTvShow.Key,
            MediaType = downloadTaskTvShow.MediaType,
            Percentage = children.Any() ? children.Average(x => x.Percentage) : 0,
            PlexLibrary = downloadTaskTvShow.PlexLibrary,
            PlexLibraryId = downloadTaskTvShow.PlexLibraryId,
            PlexServer = downloadTaskTvShow.PlexServer,
            PlexServerId = downloadTaskTvShow.PlexServerId,
        };
    }

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskTvShowSeason downloadTaskTvShowSeason)
    {
        if (downloadTaskTvShowSeason is null)
            throw new ArgumentNullException(nameof(downloadTaskTvShowSeason));

        var children = downloadTaskTvShowSeason.Children.Select(x =>
            {
                x.Calculate();
                return x.ToGeneric();
            })
            .ToList();

        return new DownloadTaskGeneric
        {
            Children = children,
            CreatedAt = downloadTaskTvShowSeason.CreatedAt,
            DataReceived = children.Sum(x => x.DataReceived),
            DataTotal = children.Sum(x => x.DataTotal),
            DestinationDirectory = string.Empty,
            DownloadDirectory = string.Empty,
            DownloadSpeed = children.Sum(x => x.DownloadSpeed),
            DownloadStatus = downloadTaskTvShowSeason.DownloadStatus,
            DownloadTaskType = downloadTaskTvShowSeason.DownloadTaskType,
            DownloadWorkerTasks = new List<DownloadWorkerTask>(),
            FileLocationUrl = string.Empty,
            FileName = string.Empty,
            FileTransferSpeed = children.Sum(x => x.FileTransferSpeed),
            Title = downloadTaskTvShowSeason.Title,
            FullTitle = downloadTaskTvShowSeason.FullTitle,
            Id = downloadTaskTvShowSeason.Id,
            IsDownloadable = downloadTaskTvShowSeason.IsDownloadable,
            Key = downloadTaskTvShowSeason.Key,
            MediaType = downloadTaskTvShowSeason.MediaType,
            Percentage = children.Any() ? children.Average(x => x.Percentage) : 0,
            PlexLibrary = downloadTaskTvShowSeason.PlexLibrary,
            PlexLibraryId = downloadTaskTvShowSeason.PlexLibraryId,
            PlexServer = downloadTaskTvShowSeason.PlexServer,
            PlexServerId = downloadTaskTvShowSeason.PlexServerId,
        };
    }

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskTvShowEpisode downloadTaskTvShowEpisode)
    {
        if (downloadTaskTvShowEpisode is null)
            throw new ArgumentNullException(nameof(downloadTaskTvShowEpisode));

        return new DownloadTaskGeneric
        {
            Children = downloadTaskTvShowEpisode.Children.Select(x => x.ToGeneric()).ToList(),
            CreatedAt = downloadTaskTvShowEpisode.CreatedAt,
            DataReceived = downloadTaskTvShowEpisode.Children.Sum(x => x.DataReceived),
            DataTotal = downloadTaskTvShowEpisode.Children.Sum(x => x.DataTotal),
            DestinationDirectory = string.Empty,
            DownloadDirectory = string.Empty,
            DownloadSpeed = downloadTaskTvShowEpisode.Children.Sum(x => x.DownloadSpeed),
            DownloadStatus = downloadTaskTvShowEpisode.DownloadStatus,
            DownloadTaskType = downloadTaskTvShowEpisode.DownloadTaskType,
            DownloadWorkerTasks = new List<DownloadWorkerTask>(),
            FileLocationUrl = string.Empty,
            FileName = string.Empty,
            FileTransferSpeed = downloadTaskTvShowEpisode.Children.Sum(x => x.FileTransferSpeed),
            Title = downloadTaskTvShowEpisode.Title,
            FullTitle = downloadTaskTvShowEpisode.FullTitle,
            Id = downloadTaskTvShowEpisode.Id,
            IsDownloadable = downloadTaskTvShowEpisode.IsDownloadable,
            Key = downloadTaskTvShowEpisode.Key,
            MediaType = downloadTaskTvShowEpisode.MediaType,
            Percentage = downloadTaskTvShowEpisode.Children.Any() ? downloadTaskTvShowEpisode.Children.Average(x => x.Percentage) : 0,
            PlexLibrary = downloadTaskTvShowEpisode.PlexLibrary,
            PlexLibraryId = downloadTaskTvShowEpisode.PlexLibraryId,
            PlexServer = downloadTaskTvShowEpisode.PlexServer,
            PlexServerId = downloadTaskTvShowEpisode.PlexServerId,
        };
    }

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskTvShowEpisodeFile downloadTaskTvShowEpisodeFile) => new()
    {
        Children = new List<DownloadTaskGeneric>(),
        CreatedAt = downloadTaskTvShowEpisodeFile.CreatedAt,
        DataReceived = downloadTaskTvShowEpisodeFile.DataReceived,
        DataTotal = downloadTaskTvShowEpisodeFile.DataTotal,
        DestinationDirectory = downloadTaskTvShowEpisodeFile.DestinationDirectory,
        DownloadDirectory = downloadTaskTvShowEpisodeFile.DownloadDirectory,
        DownloadSpeed = downloadTaskTvShowEpisodeFile.DownloadSpeed,
        DownloadStatus = downloadTaskTvShowEpisodeFile.DownloadStatus,
        DownloadTaskType = downloadTaskTvShowEpisodeFile.DownloadTaskType,
        DownloadWorkerTasks = downloadTaskTvShowEpisodeFile.DownloadWorkerTasks,
        FileLocationUrl = downloadTaskTvShowEpisodeFile.FileLocationUrl,
        FileName = downloadTaskTvShowEpisodeFile.FileName,
        FileTransferSpeed = downloadTaskTvShowEpisodeFile.FileTransferSpeed,
        Title = downloadTaskTvShowEpisodeFile.Title,
        FullTitle = downloadTaskTvShowEpisodeFile.FullTitle,
        Id = downloadTaskTvShowEpisodeFile.Id,
        IsDownloadable = downloadTaskTvShowEpisodeFile.IsDownloadable,
        Key = downloadTaskTvShowEpisodeFile.Key,
        MediaType = downloadTaskTvShowEpisodeFile.MediaType,
        Percentage = downloadTaskTvShowEpisodeFile.Percentage,
        PlexLibrary = downloadTaskTvShowEpisodeFile.PlexLibrary,
        PlexLibraryId = downloadTaskTvShowEpisodeFile.PlexLibraryId,
        PlexServer = downloadTaskTvShowEpisodeFile.PlexServer,
        PlexServerId = downloadTaskTvShowEpisodeFile.PlexServerId,
    };
}