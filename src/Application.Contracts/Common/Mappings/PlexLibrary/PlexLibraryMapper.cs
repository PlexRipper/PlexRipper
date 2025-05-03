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
            DefaultDestinationId = plexLibrary.DefaultDestinationId ?? plexLibrary.Type.ToDefaultDestinationFolderId(),
            PlexServerId = plexLibrary.PlexServerId,
            Count = plexLibrary.MediaCount,
            SeasonCount = plexLibrary.SeasonCount,
            EpisodeCount = plexLibrary.EpisodeCount,
        };

    public static List<PlexLibraryDTO> ToDTO(this List<PlexLibrary> plexLibraries) =>
        plexLibraries.Select(ToDTO).ToList();

    public static PlexMediaMetadataDTO ToMetaDataDTO(this PlexLibrary plexLibrary) =>
        new()
        {
            Roles = plexLibrary.Roles.ToDTO(),
            Countries = plexLibrary.Countries.ToDTO(),
            Genres = plexLibrary.Genres.ToDTO(),
        };

    public static List<PlexRoleDTO> ToDTO(this IEnumerable<PlexRole> roles) =>
        roles.Select(x => new PlexRoleDTO { Id = x.Id, Name = x.Name }).ToList();

    public static List<PlexCountryDTO> ToDTO(this IEnumerable<PlexCountry> countries) =>
        countries.Select(x => new PlexCountryDTO { Id = x.Id, Name = x.Name }).ToList();

    public static List<PlexGenreDTO> ToDTO(this IEnumerable<PlexGenre> genres) =>
        genres.Select(x => new PlexGenreDTO { Id = x.Id, Name = x.Name }).ToList();

    #endregion
}
