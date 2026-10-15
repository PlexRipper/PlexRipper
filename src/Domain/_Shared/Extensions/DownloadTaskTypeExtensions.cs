namespace Reaparr.Domain;

public static class DownloadTaskTypeExtensions
{
    public static bool IsParent(this DownloadTaskType downloadTaskType) =>
        downloadTaskType
            is DownloadTaskType.TvShow
                or DownloadTaskType.Season
                or DownloadTaskType.MusicArtist
                or DownloadTaskType.MusicAlbum
                or DownloadTaskType.PhotoAlbum;
}
