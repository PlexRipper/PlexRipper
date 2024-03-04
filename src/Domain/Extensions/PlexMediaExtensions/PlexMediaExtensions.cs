namespace PlexRipper.Domain.PlexMediaExtensions;

public static class PlexMediaExtensions
{
    public static DownloadTaskMovie MapToDownloadTask(this PlexMovie plexMovie) => new()
    {
        Id = default,
        Key = plexMovie.Key,
        DataTotal = 0,
        DownloadStatus = DownloadStatus.Queued,
        Created = default,
        PlexServer = null,
        PlexServerId = plexMovie.PlexServerId,
        PlexLibrary = null,
        PlexLibraryId = plexMovie.PlexLibraryId,
        Title = null,
        Year = plexMovie.Year,
        FullTitle = $"{plexMovie.Title} ({plexMovie.Year})",
        Percentage = 0,
        DataReceived = 0,
        DownloadSpeed = 0,
        Children = null,
    };

    public static List<DownloadTaskMovieFile> MapToDownloadTask(this PlexMediaData plexMediaData, PlexMovie plexMovie)
    {
        return plexMediaData.Parts.Select(part => new DownloadTaskMovieFile
            {
                Id = default,
                Key = 0,
                DataTotal = part.Size,
                DownloadStatus = DownloadStatus.Unknown,
                Created = default,
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
                DestinationFolder = null,
                DestinationFolderId = 0,
                DownloadFolder = null,
                DownloadFolderId = 0,
                DownloadWorkerTasks = null,
                Parent = null,
                ParentId = default,
            })
            .ToList();
    }
}