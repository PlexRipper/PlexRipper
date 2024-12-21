using PlexRipper.Domain;

namespace Application.Contracts;

public static class PlexMediaExtensions
{
    public static DownloadTaskMovie MapToDownloadTask(this PlexMovie plexMovie) =>
        new()
        {
            Id = default,
            Key = plexMovie.Key,
            DataTotal = 0,
            DownloadStatus = DownloadStatus.Queued,
            CreatedAt = DateTime.UtcNow,
            PlexServer = null,
            PlexServerId = plexMovie.PlexServerId,
            PlexLibrary = null,
            PlexLibraryId = plexMovie.PlexLibraryId,
            Title = plexMovie.Title,
            Year = plexMovie.Year,
            FullTitle = plexMovie.FullTitle,
            DataReceived = 0,
            DownloadSpeed = 0,
            FileDataTransferred = 0,
            FileTransferSpeed = 0,
            Children = [],
        };

    public static DownloadTaskTvShow MapToDownloadTask(this PlexTvShow plexTvShow) =>
        new()
        {
            Id = default,
            Key = plexTvShow.Key,
            DataTotal = 0,
            DownloadStatus = DownloadStatus.Queued,
            CreatedAt = DateTime.UtcNow,
            PlexServer = null,
            PlexServerId = plexTvShow.PlexServerId,
            PlexLibrary = null,
            PlexLibraryId = plexTvShow.PlexLibraryId,
            Title = plexTvShow.Title,
            Year = plexTvShow.Year,
            FullTitle = plexTvShow.FullTitle,
            DataReceived = 0,
            DownloadSpeed = 0,
            Children = [],
            FileTransferSpeed = 0,
            FileDataTransferred = 0,
        };

    public static DownloadTaskTvShowSeason MapToDownloadTask(this PlexTvShowSeason plexTvShowSeason) =>
        new()
        {
            Id = default,
            Key = plexTvShowSeason.Key,
            DataTotal = 0,
            DownloadStatus = DownloadStatus.Queued,
            CreatedAt = DateTime.UtcNow,
            PlexServer = null,
            PlexServerId = plexTvShowSeason.PlexServerId,
            PlexLibrary = null,
            PlexLibraryId = plexTvShowSeason.PlexLibraryId,
            Title = plexTvShowSeason.Title,
            Year = plexTvShowSeason.Year,
            FullTitle = plexTvShowSeason.FullTitle,
            DataReceived = 0,
            DownloadSpeed = 0,
            Children = [],
            ParentId = default,
            Parent = null,
            FileTransferSpeed = 0,
            FileDataTransferred = 0,
        };

    public static DownloadTaskTvShowEpisode MapToDownloadTask(this PlexTvShowEpisode plexTvShowEpisode) =>
        new()
        {
            Id = default,
            Key = plexTvShowEpisode.Key,
            DataTotal = 0,
            DownloadStatus = DownloadStatus.Queued,
            CreatedAt = DateTime.UtcNow,
            PlexServer = null,
            PlexServerId = plexTvShowEpisode.PlexServerId,
            PlexLibrary = null,
            PlexLibraryId = plexTvShowEpisode.PlexLibraryId,
            Title = plexTvShowEpisode.Title,
            Year = plexTvShowEpisode.Year,
            FullTitle = plexTvShowEpisode.FullTitle,
            DataReceived = 0,
            DownloadSpeed = 0,
            Children = [],
            ParentId = default,
            Parent = null,
            FileTransferSpeed = 0,
            FileDataTransferred = 0,
        };

    public static List<DownloadTaskMovieFile> MapToDownloadTask(
        this PlexMediaData plexMediaData,
        PlexMovie plexMovie,
        CreateDownloadTasksRequest request
    )
    {
        return plexMediaData
            .Parts.Select(part => new DownloadTaskMovieFile
            {
                Id = default,
                Key = 0,
                DataTotal = part.Size,
                DownloadStatus = DownloadStatus.Queued,
                CreatedAt = DateTime.UtcNow,
                PlexServer = null,
                PlexServerId = plexMovie.PlexServerId,
                PlexLibrary = null,
                PlexLibraryId = plexMovie.PlexLibraryId,
                DataReceived = 0,
                DownloadSpeed = 0,
                FileTransferSpeed = 0,
                FileDataTransferred = 0,
                FileName = part.File.GetFileName(),
                FileLocationUrl = part.ObfuscatedFilePath,
                Quality = plexMediaData.VideoResolution,
                DirectoryMeta = new DownloadTaskDirectory()
                {
                    DownloadRootPath = string.Empty,
                    DestinationRootPath = string.Empty,
                    MovieFolder = plexMovie.Title.SanitizeFolderName(),
                    TvShowFolder = string.Empty,
                    SeasonFolder = string.Empty,
                },
                DownloadWorkerTasks = [],
                Parent = null,
                ParentId = default,
                DestinationFolderPathId = request.DestinationFolderPathId,
                FullTitle = $"{plexMovie.FullTitle}/{part.File.GetFileName()}",
                Title = part.File.GetFileName(),
            })
            .ToList();
    }

    public static List<DownloadTaskTvShowEpisodeFile> MapToDownloadTask(
        this PlexMediaData plexMediaData,
        PlexTvShowEpisode plexTvShowEpisode,
        CreateDownloadTasksRequest request
    )
    {
        if (plexTvShowEpisode.TvShow is null || plexTvShowEpisode.TvShowSeason is null)
        {
            throw new NullReferenceException("PlexTvShowEpisode.TvShow or PlexTvShowEpisode.TvShowSeason is null");
        }

        return plexMediaData
            .Parts.Select(part => new DownloadTaskTvShowEpisodeFile
            {
                Id = default,
                Key = 0,
                DataTotal = part.Size,
                DownloadStatus = DownloadStatus.Queued,
                CreatedAt = DateTime.UtcNow,
                PlexServer = null,
                PlexServerId = plexTvShowEpisode.PlexServerId,
                PlexLibrary = null,
                PlexLibraryId = plexTvShowEpisode.PlexLibraryId,
                DataReceived = 0,
                DownloadSpeed = 0,
                FileTransferSpeed = 0,
                FileDataTransferred = 0,
                FileName = part.File.GetFileName(),
                FileLocationUrl = part.ObfuscatedFilePath,
                Quality = plexMediaData.VideoResolution,
                DirectoryMeta = new DownloadTaskDirectory()
                {
                    DownloadRootPath = string.Empty,
                    DestinationRootPath = string.Empty,
                    MovieFolder = string.Empty,
                    TvShowFolder = plexTvShowEpisode.TvShow.Title.SanitizeFolderName(),
                    SeasonFolder = plexTvShowEpisode.TvShowSeason.Title.SanitizeFolderName(),
                },
                DownloadWorkerTasks = [],
                Parent = null,
                ParentId = default,
                DestinationFolderPathId = request.DestinationFolderPathId,
                FullTitle = $"{plexTvShowEpisode.FullTitle}/{part.File.GetFileName()}",
                Title = part.File.GetFileName(),
            })
            .ToList();
    }
}
