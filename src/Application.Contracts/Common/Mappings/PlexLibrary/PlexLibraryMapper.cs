using PlexRipper.Domain;

namespace Application.Contracts;

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
            DefaultDestinationId = plexLibrary.DefaultDestinationId ?? 0,
            PlexServerId = plexLibrary.PlexServerId,
            Count = plexLibrary.MediaCount,
            SeasonCount = plexLibrary.SeasonCount,
            EpisodeCount = plexLibrary.EpisodeCount,
        };

    public static List<PlexLibraryDTO> ToDTO(this List<PlexLibrary> plexLibraries) =>
        plexLibraries.Select(ToDTO).ToList();

    #endregion
}
