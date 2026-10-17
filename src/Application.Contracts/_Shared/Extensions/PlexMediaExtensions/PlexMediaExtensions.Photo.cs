namespace Reaparr.Application.Contracts;

public static partial class PlexMediaExtensions
{
    public static DownloadTaskPhotoImageFile MapToDownloadTask(
        this PlexPhotoMediaData mediaData,
        DownloadTaskPhotoFolder parent,
        CreateDownloadTasksRequest request,
        string downloadRootPath,
        string destinationRootPath,
        string relativeDirectory,
        bool keepCompletedInDownloadFolder
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            PlexApiRatingKey = mediaData.PlexApiRatingKey,
            PlexApiMediaId = mediaData.PlexApiMediaId,
            PlexApiPartId = mediaData.PlexApiPartId,
            HashId = null,
            AssetKind = mediaData.AssetKind,
            DataTotal = mediaData.Size,
            DownloadStatus = DownloadStatus.Queued,
            CreatedAt = DateTime.UtcNow,
            PlexServer = null,
            PlexServerId = mediaData.PlexServerId,
            PlexLibrary = null,
            PlexLibraryId = mediaData.PlexLibraryId,
            DataReceived = 0,
            DownloadSpeed = 0,
            FileTransferSpeed = 0,
            FileDataTransferred = 0,
            TimeRemaining = 0,
            FileName = mediaData.OriginalFilename.GetFileName(),
            FileLocationUrl = mediaData.Key,
            VideoQuality = VideoQuality.Unknown,
            DirectoryMeta = new DownloadTaskDirectory
            {
                DownloadRootPath = downloadRootPath,
                DestinationRootPath = destinationRootPath,
                MovieFolder = string.Empty,
                TvShowFolder = string.Empty,
                SeasonFolder = string.Empty,
                MusicArtistFolder = string.Empty,
                MusicAlbumFolder = string.Empty,
                PhotoFolder = relativeDirectory,
                OtherVideoFolder = string.Empty,
                KeepCompletedInDownloadFolder = keepCompletedInDownloadFolder,
            },
            Parent = parent,
            ParentId = parent.Id,
            DestinationFolderPathId = request.DestinationFolderPathId,
            FullTitle = $"{parent.FullTitle}/{mediaData.OriginalFilename.GetFileName()}",
            Title = mediaData.OriginalFilename.GetFileName(),
            DirectDownloadSnapshot = null,
            DownloadClientType = PlexDownloadClientType.Direct,
            SonarrIntegrationId = request.Integration?.Type == IntegrationType.Sonarr ? request.Integration.Id : null,
            RadarrIntegrationId = request.Integration?.Type == IntegrationType.Radarr ? request.Integration.Id : null,
        };

}
