namespace Reaparr.Application.Contracts;

public record PlexLibraryDTO
{
    public required int Id { get; set; }

    public required string Key { get; set; }

    public required string Title { get; set; }

    public required PlexMediaType Type { get; set; }

    public required DateTime? UpdatedAt { get; set; }

    public required DateTime? CreatedAt { get; set; }

    public required DateTime? ScannedAt { get; set; }

    public required DateTime? SyncedAt { get; set; }

    public required bool Outdated { get; set; }

    public required string Uuid { get; set; }

    public required long MediaSize { get; set; }

    public required FolderPathDTO? DefaultDestination { get; set; }

    public required int DefaultDestinationId { get; set; }

    public required int PlexServerId { get; set; }

    public required int Count { get; set; }

    public required int MusicArtistCount { get; set; }

    public required int MusicAlbumCount { get; set; }

    public required int MusicTrackCount { get; set; }

    public required int PhotoAlbumCount { get; set; }

    public required int PhotoImageCount { get; set; }

    public required int PhotoClipCount { get; set; }

    public required int OtherVideoCount { get; set; }

    public required int SeasonCount { get; set; }

    public required int EpisodeCount { get; set; }

    public required bool IsEnabled { get; set; }
}
