using Reaparr.Domain;

namespace Reaparr.Data.Contracts;

public static class DownloadTaskGenericMapper
{
    #region Movie

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskMovie downloadTaskMovie)
    {
        var children = downloadTaskMovie.Children.Select(x => x.ToGeneric()).ToList();
        var child = children.FirstOrDefault();

        // TODO: calculate destination and download directory for parents instead of relying on children because those are not always retrieved

        var generic = new DownloadTaskGeneric
        {
            Id = downloadTaskMovie.Id,
            MediaKey = downloadTaskMovie.Key,
            Title = downloadTaskMovie.Title,
            FullTitle = downloadTaskMovie.FullTitle,
            MediaType = downloadTaskMovie.MediaType,
            DownloadTaskType = downloadTaskMovie.DownloadTaskType,
            DownloadStatus = downloadTaskMovie.DownloadStatus,
            DataReceived = downloadTaskMovie.DataReceived,
            DataTotal = downloadTaskMovie.DataTotal,
            CreatedAt = downloadTaskMovie.CreatedAt,
            FileName = string.Empty,
            IsDownloadable = downloadTaskMovie.IsDownloadable,
            DownloadDirectory = child?.DownloadDirectory ?? string.Empty,
            DestinationDirectory = child?.DestinationDirectory ?? string.Empty,
            Quality = string.Empty,
            FileLocationUrl = string.Empty,
            DownloadSpeed = downloadTaskMovie.DownloadSpeed,
            FileTransferSpeed = downloadTaskMovie.FileTransferSpeed,
            FileDataTransferred = downloadTaskMovie.FileDataTransferred,
            CurrentFileTransferBytesOffset = 0,
            Children = children,
            DownloadWorkerTasks = [],
            ParentId = Guid.Empty,
            PlexServer = downloadTaskMovie.PlexServer,
            PlexServerId = downloadTaskMovie.PlexServerId,
            PlexLibrary = downloadTaskMovie.PlexLibrary,
            PlexLibraryId = downloadTaskMovie.PlexLibraryId,
        };

        generic.Calculate();

        return generic;
    }

    #endregion

    #region MovieFile

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskMovieFile file)
    {
        var downloadTaskGeneric = new DownloadTaskGeneric
        {
            Id = file.Id,
            MediaKey = file.Key,
            Title = file.Title,
            FullTitle = file.FullTitle,
            MediaType = file.MediaType,
            DownloadTaskType = file.DownloadTaskType,
            DownloadStatus = file.DownloadStatus,
            DataReceived = file.DataReceived,
            DataTotal = file.DataTotal,
            CreatedAt = file.CreatedAt,
            FileName = file.FileName,
            IsDownloadable = file.IsDownloadable,
            DownloadDirectory = file.DownloadDirectory,
            DestinationDirectory = file.DestinationDirectory,
            FileLocationUrl = file.FileLocationUrl,
            DownloadSpeed = file.DownloadSpeed,
            FileTransferSpeed = file.FileTransferSpeed,
            Children = [],
            Quality = file.Quality,
            DownloadWorkerTasks = file.DownloadWorkerTasks.ToList(),
            ParentId = file.ParentId,
            PlexServer = file.PlexServer,
            PlexServerId = file.PlexServerId,
            PlexLibrary = file.PlexLibrary,
            PlexLibraryId = file.PlexLibraryId,
            FileDataTransferred = file.FileDataTransferred,
            CurrentFileTransferBytesOffset = file.CurrentFileTransferBytesOffset,
        };
        return downloadTaskGeneric;
    }

    #endregion

    #region TvShow

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskTvShow downloadTaskTvShow)
    {
        var children = downloadTaskTvShow.Children.Select(x => x.ToGeneric()).ToList();
        var child = children.FirstOrDefault();

        var generic = new DownloadTaskGeneric
        {
            Id = downloadTaskTvShow.Id,
            MediaKey = downloadTaskTvShow.Key,
            Title = downloadTaskTvShow.Title,
            FullTitle = downloadTaskTvShow.FullTitle,
            MediaType = downloadTaskTvShow.MediaType,
            DownloadTaskType = downloadTaskTvShow.DownloadTaskType,
            DownloadStatus = downloadTaskTvShow.DownloadStatus,
            DataReceived = downloadTaskTvShow.DataReceived,
            DataTotal = downloadTaskTvShow.DataTotal,
            CreatedAt = downloadTaskTvShow.CreatedAt,
            FileName = string.Empty,
            Quality = string.Empty,
            IsDownloadable = downloadTaskTvShow.IsDownloadable,
            DownloadDirectory = Path.GetDirectoryName(child?.DownloadDirectory) ?? string.Empty,
            DestinationDirectory = Path.GetDirectoryName(child?.DestinationDirectory) ?? string.Empty,
            FileLocationUrl = string.Empty,
            DownloadSpeed = downloadTaskTvShow.DownloadSpeed,
            FileTransferSpeed = downloadTaskTvShow.FileTransferSpeed,
            FileDataTransferred = downloadTaskTvShow.FileDataTransferred,
            Children = children,
            DownloadWorkerTasks = [],
            ParentId = Guid.Empty,
            PlexServer = downloadTaskTvShow.PlexServer,
            PlexServerId = downloadTaskTvShow.PlexServerId,
            PlexLibrary = downloadTaskTvShow.PlexLibrary,
            PlexLibraryId = downloadTaskTvShow.PlexLibraryId,
            CurrentFileTransferBytesOffset = 0,
        };

        generic.Calculate();

        return generic;
    }

    #endregion

    #region Season

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskTvShowSeason downloadTaskTvShowSeason)
    {
        var children = downloadTaskTvShowSeason.Children.Select(x => x.ToGeneric()).ToList();
        var child = children.FirstOrDefault();

        var generic = new DownloadTaskGeneric
        {
            Id = downloadTaskTvShowSeason.Id,
            MediaKey = downloadTaskTvShowSeason.Key,
            Title = downloadTaskTvShowSeason.Title,
            FullTitle = downloadTaskTvShowSeason.FullTitle,
            MediaType = downloadTaskTvShowSeason.MediaType,
            DownloadTaskType = downloadTaskTvShowSeason.DownloadTaskType,
            DownloadStatus = downloadTaskTvShowSeason.DownloadStatus,
            DataReceived = downloadTaskTvShowSeason.DataReceived,
            DataTotal = downloadTaskTvShowSeason.DataTotal,
            CreatedAt = downloadTaskTvShowSeason.CreatedAt,
            FileName = string.Empty,
            IsDownloadable = downloadTaskTvShowSeason.IsDownloadable,
            DownloadDirectory = child?.DownloadDirectory ?? string.Empty,
            DestinationDirectory = child?.DestinationDirectory ?? string.Empty,
            FileDataTransferred = downloadTaskTvShowSeason.FileDataTransferred,
            FileLocationUrl = string.Empty,
            Quality = string.Empty,
            DownloadSpeed = downloadTaskTvShowSeason.DownloadSpeed,
            FileTransferSpeed = downloadTaskTvShowSeason.FileTransferSpeed,
            Children = children,
            DownloadWorkerTasks = [],
            ParentId = downloadTaskTvShowSeason.ParentId,
            PlexServer = downloadTaskTvShowSeason.PlexServer,
            PlexServerId = downloadTaskTvShowSeason.PlexServerId,
            PlexLibrary = downloadTaskTvShowSeason.PlexLibrary,
            PlexLibraryId = downloadTaskTvShowSeason.PlexLibraryId,
            CurrentFileTransferBytesOffset = 0,
        };

        generic.Calculate();

        return generic;
    }

    #endregion

    #region Episode

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskTvShowEpisode downloadTaskTvShowEpisode)
    {
        var children = downloadTaskTvShowEpisode.Children.Select(x => x.ToGeneric()).ToList();
        var child = children.FirstOrDefault();

        var generic = new DownloadTaskGeneric
        {
            Id = downloadTaskTvShowEpisode.Id,
            MediaKey = downloadTaskTvShowEpisode.Key,
            Title = downloadTaskTvShowEpisode.Title,
            FullTitle = downloadTaskTvShowEpisode.FullTitle,
            MediaType = downloadTaskTvShowEpisode.MediaType,
            DownloadTaskType = downloadTaskTvShowEpisode.DownloadTaskType,
            DownloadStatus = downloadTaskTvShowEpisode.DownloadStatus,
            FileDataTransferred = downloadTaskTvShowEpisode.FileDataTransferred,
            DataReceived = downloadTaskTvShowEpisode.DataReceived,
            DataTotal = downloadTaskTvShowEpisode.DataTotal,
            CreatedAt = downloadTaskTvShowEpisode.CreatedAt,
            FileName = string.Empty,
            Quality = string.Empty,
            IsDownloadable = downloadTaskTvShowEpisode.IsDownloadable,
            DownloadDirectory = child?.DownloadDirectory ?? string.Empty,
            DestinationDirectory = child?.DestinationDirectory ?? string.Empty,
            FileLocationUrl = string.Empty,
            DownloadSpeed = downloadTaskTvShowEpisode.DownloadSpeed,
            FileTransferSpeed = downloadTaskTvShowEpisode.FileTransferSpeed,
            Children = children,
            DownloadWorkerTasks = [],
            ParentId = downloadTaskTvShowEpisode.ParentId,
            PlexServer = downloadTaskTvShowEpisode.PlexServer,
            PlexServerId = downloadTaskTvShowEpisode.PlexServerId,
            PlexLibrary = downloadTaskTvShowEpisode.PlexLibrary,
            PlexLibraryId = downloadTaskTvShowEpisode.PlexLibraryId,
            CurrentFileTransferBytesOffset = 0,
        };

        generic.Calculate();

        return generic;
    }

    #endregion

    #region EpisodeFile

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskTvShowEpisodeFile file) =>
        new()
        {
            Id = file.Id,
            MediaKey = file.Key,
            Title = file.Title,
            FullTitle = file.FullTitle,
            MediaType = file.MediaType,
            DownloadTaskType = file.DownloadTaskType,
            DownloadStatus = file.DownloadStatus,
            FileDataTransferred = file.FileDataTransferred,
            DataReceived = file.DataReceived,
            DataTotal = file.DataTotal,
            CreatedAt = file.CreatedAt,
            FileName = file.FileName,
            IsDownloadable = file.IsDownloadable,
            DownloadDirectory = file.DownloadDirectory,
            DestinationDirectory = file.DestinationDirectory,
            FileLocationUrl = file.FileLocationUrl,
            DownloadSpeed = file.DownloadSpeed,
            FileTransferSpeed = file.FileTransferSpeed,
            Children = [],
            Quality = string.Empty,
            DownloadWorkerTasks = file.DownloadWorkerTasks.ToList(),
            ParentId = file.ParentId,
            PlexServer = file.PlexServer,
            PlexServerId = file.PlexServerId,
            PlexLibrary = file.PlexLibrary,
            PlexLibraryId = file.PlexLibraryId,
            CurrentFileTransferBytesOffset = file.CurrentFileTransferBytesOffset,
        };

    #endregion
}
