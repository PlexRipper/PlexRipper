namespace Reaparr.Application.Contracts;

public static class PlexLibraryMapper
{
    #region ToDTO

    public static PlexLibraryDTO ToDTO(this PlexLibrary plexLibrary) =>
        new()
        {
            Id = plexLibrary.Id,
            Key = plexLibrary.Key,
            Title = plexLibrary.Title,
            Type = plexLibrary.Type,
            UpdatedAt = plexLibrary.UpdatedAt,
            CreatedAt = plexLibrary.CreatedAt,
            ScannedAt = plexLibrary.ScannedAt,
            SyncedAt = plexLibrary.SyncedAt,
            Outdated = plexLibrary.Outdated,
            Uuid = plexLibrary.Uuid,
            MediaSize = plexLibrary.MediaSize,
            DefaultDestination = plexLibrary.DefaultDestination?.ToDTO() ?? null,
            DefaultDestinationId = plexLibrary.DefaultDestinationId ?? plexLibrary.Type.ToDefaultDestinationFolderId(),
            PlexServerId = plexLibrary.PlexServerId,
            Count = plexLibrary.MediaCount,
            MusicArtistCount = plexLibrary.MusicArtistCount,
            MusicAlbumCount = plexLibrary.MusicAlbumCount,
            MusicTrackCount = plexLibrary.MusicTrackCount,
            PhotoAlbumCount = plexLibrary.PhotoAlbumCount,
            PhotoImageCount = plexLibrary.PhotoImageCount,
            PhotoClipCount = plexLibrary.PhotoClipCount,
            OtherVideoCount = plexLibrary.OtherVideoCount,
            SeasonCount = plexLibrary.SeasonCount,
            EpisodeCount = plexLibrary.EpisodeCount,
            IsEnabled = plexLibrary.IsEnabled,
        };

    public static List<PlexLibraryDTO> ToDTO(this List<PlexLibrary> plexLibraries) =>
        plexLibraries.Select(ToDTO).ToList();

    #endregion
}
