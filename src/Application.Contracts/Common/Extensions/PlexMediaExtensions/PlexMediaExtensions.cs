using PlexRipper.Domain;

namespace Application.Contracts;

public static class PlexMediaExtensions
{
    public static void SetFullThumbnailUrl(
        this PlexMediaSlim plexMediaSlim,
        string connectionUrl,
        string plexServerToken
    )
    {
        if (
            !plexMediaSlim.HasThumb
            || connectionUrl == string.Empty
            || plexMediaSlim.ThumbUrl == string.Empty
            || plexServerToken == string.Empty
        )
            return;

        plexMediaSlim.FullThumbUrl =
            $"{connectionUrl}/photo/:/transcode?url={plexMediaSlim.ThumbUrl}&X-Plex-Token={plexServerToken}";
        plexMediaSlim.HasThumb = true;
    }

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
        FileTransferSpeed = 0,
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
        FileTransferSpeed = 0,
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
        ParentId = default,
        Parent = null,
        FileTransferSpeed = 0,
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
        ParentId = default,
        Parent = null,
        FileTransferSpeed = 0,
    };

    public static List<DownloadTaskMovieFile> MapToDownloadTask(this PlexMediaData plexMediaData, PlexMovie plexMovie)
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
                Percentage = 0,
                DataReceived = 0,
                DownloadSpeed = 0,
                FileTransferSpeed = 0,
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
                DownloadWorkerTasks = new List<DownloadWorkerTask>(),
                Parent = null,
                ParentId = default,
                FullTitle = $"{plexMovie.FullTitle}/{part.File.GetFileName()}",
                Title = part.File.GetFileName(),
            })
            .ToList();
    }

    public static List<DownloadTaskTvShowEpisodeFile> MapToDownloadTask(
        this PlexMediaData plexMediaData,
        PlexTvShowEpisode plexTvShowEpisode
    )
    {
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
                Percentage = 0,
                DataReceived = 0,
                DownloadSpeed = 0,
                FileTransferSpeed = 0,
                FileName = part.File.GetFileName(),
                FileLocationUrl = part.ObfuscatedFilePath,
                Quality = plexMediaData.VideoResolution,
                DirectoryMeta = new DownloadTaskDirectory()
                {
                    DownloadRootPath = string.Empty,
                    DestinationRootPath = string.Empty,
                    MovieFolder = string.Empty,
                    TvShowFolder = plexTvShowEpisode.TvShow?.Title.SanitizeFolderName(),
                    SeasonFolder = plexTvShowEpisode.TvShowSeason?.Title.SanitizeFolderName(),
                },
                DownloadWorkerTasks = [],
                Parent = null,
                ParentId = default,
                FullTitle = $"{plexTvShowEpisode.FullTitle}/{part.File.GetFileName()}",
                Title = part.File.GetFileName(),
            })
            .ToList();
    }
}