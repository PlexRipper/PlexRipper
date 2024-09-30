using PlexRipper.Domain;

namespace Data.Contracts;

public static class DownloadTaskGenericMapper
{
    #region Movie

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskMovie downloadTaskMovie)
    {
        var children = downloadTaskMovie.Children.Select(x => x.ToGeneric()).ToList();
        var child = children.FirstOrDefault();

        // TODO calculate destination and download directory for parents instead of relying on children because those are not always retrieved

        var generic = new DownloadTaskGeneric
        {
            Id = downloadTaskMovie.Id,
            MediaKey = downloadTaskMovie.Key,
            Title = downloadTaskMovie.Title,
            FullTitle = downloadTaskMovie.FullTitle,
            MediaType = downloadTaskMovie.MediaType,
            DownloadTaskType = downloadTaskMovie.DownloadTaskType,
            DownloadStatus = downloadTaskMovie.DownloadStatus,
            Percentage = downloadTaskMovie.Percentage,
            DataReceived = downloadTaskMovie.DataReceived,
            DataTotal = downloadTaskMovie.DataTotal,
            CreatedAt = downloadTaskMovie.CreatedAt,
            FileName = string.Empty,
            IsDownloadable = downloadTaskMovie.IsDownloadable,
            TimeRemaining = downloadTaskMovie.TimeRemaining,
            DownloadDirectory = child?.DownloadDirectory ?? string.Empty,
            DestinationDirectory = child?.DestinationDirectory ?? string.Empty,
            Quality = string.Empty,
            FileLocationUrl = string.Empty,
            DownloadSpeed = downloadTaskMovie.DownloadSpeed,
            FileTransferSpeed = downloadTaskMovie.FileTransferSpeed,
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

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskMovieFile downloadTaskMovieFile) =>
        new()
        {
            Id = downloadTaskMovieFile.Id,
            MediaKey = downloadTaskMovieFile.Key,
            Title = downloadTaskMovieFile.Title,
            FullTitle = downloadTaskMovieFile.FullTitle,
            MediaType = downloadTaskMovieFile.MediaType,
            DownloadTaskType = downloadTaskMovieFile.DownloadTaskType,
            DownloadStatus = downloadTaskMovieFile.DownloadStatus,
            Percentage = downloadTaskMovieFile.Percentage,
            DataReceived = downloadTaskMovieFile.DataReceived,
            DataTotal = downloadTaskMovieFile.DataTotal,
            CreatedAt = downloadTaskMovieFile.CreatedAt,
            FileName = downloadTaskMovieFile.FileName,
            IsDownloadable = downloadTaskMovieFile.IsDownloadable,
            TimeRemaining = downloadTaskMovieFile.TimeRemaining,
            DownloadDirectory = downloadTaskMovieFile.DownloadDirectory,
            DestinationDirectory = downloadTaskMovieFile.DestinationDirectory,
            FileLocationUrl = downloadTaskMovieFile.FileLocationUrl,
            DownloadSpeed = downloadTaskMovieFile.DownloadSpeed,
            FileTransferSpeed = downloadTaskMovieFile.FileTransferSpeed,
            Children = [],
            Quality = string.Empty,
            DownloadWorkerTasks = downloadTaskMovieFile.DownloadWorkerTasks,
            ParentId = downloadTaskMovieFile.ParentId,
            PlexServer = downloadTaskMovieFile.PlexServer,
            PlexServerId = downloadTaskMovieFile.PlexServerId,
            PlexLibrary = downloadTaskMovieFile.PlexLibrary,
            PlexLibraryId = downloadTaskMovieFile.PlexLibraryId,
        };

    #endregion

    #region TvShow

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskTvShow downloadTaskTvShow)
    {
        var children = downloadTaskTvShow.Children.Select(x => x.ToGeneric()).ToList();
        var child = children.First();

        var generic = new DownloadTaskGeneric
        {
            Id = downloadTaskTvShow.Id,
            MediaKey = downloadTaskTvShow.Key,
            Title = downloadTaskTvShow.Title,
            FullTitle = downloadTaskTvShow.FullTitle,
            MediaType = downloadTaskTvShow.MediaType,
            DownloadTaskType = downloadTaskTvShow.DownloadTaskType,
            DownloadStatus = downloadTaskTvShow.DownloadStatus,
            Percentage = downloadTaskTvShow.Percentage,
            DataReceived = downloadTaskTvShow.DataReceived,
            DataTotal = downloadTaskTvShow.DataTotal,
            CreatedAt = downloadTaskTvShow.CreatedAt,
            FileName = string.Empty,
            Quality = string.Empty,
            IsDownloadable = downloadTaskTvShow.IsDownloadable,
            TimeRemaining = downloadTaskTvShow.TimeRemaining,
            DownloadDirectory = Path.GetDirectoryName(child?.DownloadDirectory) ?? string.Empty,
            DestinationDirectory = Path.GetDirectoryName(child?.DestinationDirectory) ?? string.Empty,
            FileLocationUrl = string.Empty,
            DownloadSpeed = downloadTaskTvShow.DownloadSpeed,
            FileTransferSpeed = downloadTaskTvShow.FileTransferSpeed,
            Children = children,
            DownloadWorkerTasks = [],
            ParentId = Guid.Empty,
            PlexServer = downloadTaskTvShow.PlexServer,
            PlexServerId = downloadTaskTvShow.PlexServerId,
            PlexLibrary = downloadTaskTvShow.PlexLibrary,
            PlexLibraryId = downloadTaskTvShow.PlexLibraryId,
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
            Percentage = downloadTaskTvShowSeason.Percentage,
            DataReceived = downloadTaskTvShowSeason.DataReceived,
            DataTotal = downloadTaskTvShowSeason.DataTotal,
            CreatedAt = downloadTaskTvShowSeason.CreatedAt,
            FileName = string.Empty,
            IsDownloadable = downloadTaskTvShowSeason.IsDownloadable,
            TimeRemaining = downloadTaskTvShowSeason.TimeRemaining,
            DownloadDirectory = child?.DownloadDirectory ?? string.Empty,
            DestinationDirectory = child?.DestinationDirectory ?? string.Empty,
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
            Percentage = downloadTaskTvShowEpisode.Percentage,
            DataReceived = downloadTaskTvShowEpisode.DataReceived,
            DataTotal = downloadTaskTvShowEpisode.DataTotal,
            CreatedAt = downloadTaskTvShowEpisode.CreatedAt,
            FileName = string.Empty,
            Quality = string.Empty,
            IsDownloadable = downloadTaskTvShowEpisode.IsDownloadable,
            TimeRemaining = downloadTaskTvShowEpisode.TimeRemaining,
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
        };

        generic.Calculate();

        return generic;
    }

    #endregion

    #region EpisodeFile

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskTvShowEpisodeFile downloadTaskTvShowEpisodeFile) =>
        new()
        {
            Id = downloadTaskTvShowEpisodeFile.Id,
            MediaKey = downloadTaskTvShowEpisodeFile.Key,
            Title = downloadTaskTvShowEpisodeFile.Title,
            FullTitle = downloadTaskTvShowEpisodeFile.FullTitle,
            MediaType = downloadTaskTvShowEpisodeFile.MediaType,
            DownloadTaskType = downloadTaskTvShowEpisodeFile.DownloadTaskType,
            DownloadStatus = downloadTaskTvShowEpisodeFile.DownloadStatus,
            Percentage = downloadTaskTvShowEpisodeFile.Percentage,
            DataReceived = downloadTaskTvShowEpisodeFile.DataReceived,
            DataTotal = downloadTaskTvShowEpisodeFile.DataTotal,
            CreatedAt = downloadTaskTvShowEpisodeFile.CreatedAt,
            FileName = downloadTaskTvShowEpisodeFile.FileName,
            IsDownloadable = downloadTaskTvShowEpisodeFile.IsDownloadable,
            TimeRemaining = downloadTaskTvShowEpisodeFile.TimeRemaining,
            DownloadDirectory = downloadTaskTvShowEpisodeFile.DownloadDirectory,
            DestinationDirectory = downloadTaskTvShowEpisodeFile.DestinationDirectory,
            FileLocationUrl = downloadTaskTvShowEpisodeFile.FileLocationUrl,
            DownloadSpeed = downloadTaskTvShowEpisodeFile.DownloadSpeed,
            FileTransferSpeed = downloadTaskTvShowEpisodeFile.FileTransferSpeed,
            Children = [],
            Quality = string.Empty,
            DownloadWorkerTasks = downloadTaskTvShowEpisodeFile.DownloadWorkerTasks ?? [],
            ParentId = downloadTaskTvShowEpisodeFile.ParentId,
            PlexServer = downloadTaskTvShowEpisodeFile.PlexServer,
            PlexServerId = downloadTaskTvShowEpisodeFile.PlexServerId,
            PlexLibrary = downloadTaskTvShowEpisodeFile.PlexLibrary,
            PlexLibraryId = downloadTaskTvShowEpisodeFile.PlexLibraryId,
        };

    #endregion
}
