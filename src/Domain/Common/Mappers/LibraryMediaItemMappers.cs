namespace PlexRipper.Domain;

public static class LibraryMediaItemMappers
{
    public static PlexActor ToPlexActor(this LibraryMediaItemRoleDTO role) =>
        new()
        {
            Name = role.Name,
            Key = role.TagKey,
            Thumb = role.Thumb,
        };

    public static List<PlexActor> ToPlexActor(this IEnumerable<LibraryMediaItemRoleDTO>? source) =>
        source?.Select(x => x.ToPlexActor()).ToList() ?? [];

    public static PlexCountry ToPlexCountry(this LibraryMediaItemCountryDTO source) =>
        new() { Name = source.Name, Key = source.Key };

    public static List<PlexCountry> ToPlexCountry(this IEnumerable<LibraryMediaItemCountryDTO>? source) =>
        source?.Select(x => x.ToPlexCountry()).ToList() ?? [];

    public static PlexGenre ToPlexGenre(this LibraryMediaItemGenreDTO source) =>
        new() { Name = source.Name, Key = source.Key };

    public static List<PlexGenre> ToPlexGenre(this IEnumerable<LibraryMediaItemGenreDTO>? source) =>
        source?.Select(x => x.ToPlexGenre()).ToList() ?? [];
}
