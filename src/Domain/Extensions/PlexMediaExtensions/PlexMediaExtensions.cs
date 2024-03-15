namespace PlexRipper.Domain.PlexMediaExtensions;

public static class PlexMediaExtensions
{
    public static DownloadTaskMovie MapToDownloadTask(this PlexMovie plexMovie) => new()
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
        Percentage = 0,
        DataReceived = 0,
        DownloadSpeed = 0,
        Children = new List<DownloadTaskMovieFile>(),
    };

    public static DownloadTaskTvShow MapToDownloadTask(this PlexTvShow plexTvShow) => new()
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
        Percentage = 0,
        DataReceived = 0,
        DownloadSpeed = 0,
        Children = new List<DownloadTaskTvShowSeason>(),
    };

    public static DownloadTaskTvShowSeason MapToDownloadTask(this PlexTvShowSeason plexTvShowSeason) => new()
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
        Percentage = 0,
        DataReceived = 0,
        DownloadSpeed = 0,
        Children = new List<DownloadTaskTvShowEpisode>(),
    };

    public static DownloadTaskTvShowEpisode MapToDownloadTask(this PlexTvShowEpisode plexTvShowEpisode) => new()
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
        Percentage = 0,
        DataReceived = 0,
        DownloadSpeed = 0,
        Children = new List<DownloadTaskTvShowEpisodeFile>(),
    };

    public static List<DownloadTaskMovieFile> MapToDownloadTask(this PlexMediaData plexMediaData, PlexMovie plexMovie)
    {
        return plexMediaData.Parts.Select(part => new DownloadTaskMovieFile
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
                Percentage = 0,
                DataReceived = 0,
                DownloadSpeed = 0,
                FileTransferSpeed = 0,
                FileName = Path.GetFileName(part.File),
                FileLocationUrl = part.ObfuscatedFilePath,
                Quality = plexMediaData.VideoResolution,
                DownloadDirectory = null,
                DestinationDirectory = null,
                DownloadWorkerTasks = null,
                Parent = null,
                ParentId = default,
                FullTitle = $"{plexMovie.FullTitle}/{Path.GetFileName(part.File)}",
                Title = part.File,
            })
            .ToList();
    }

    public static List<DownloadTaskTvShowEpisodeFile> MapToDownloadTask(this PlexMediaData plexMediaData, PlexTvShowEpisode plexTvShowEpisode)
    {
        return plexMediaData.Parts.Select(part => new DownloadTaskTvShowEpisodeFile
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
                Percentage = 0,
                DataReceived = 0,
                DownloadSpeed = 0,
                FileTransferSpeed = 0,
                FileName = Path.GetFileName(part.File),
                FileLocationUrl = part.ObfuscatedFilePath,
                Quality = plexMediaData.VideoResolution,
                DownloadDirectory = null,
                DestinationDirectory = null,
                DownloadWorkerTasks = null,
                Parent = null,
                ParentId = default,
            })
            .ToList();
    }
}