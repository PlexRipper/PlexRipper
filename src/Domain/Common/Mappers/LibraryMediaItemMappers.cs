namespace PlexRipper.Domain;

public static class LibraryMediaItemMappers
{
    public static PlexRole ToPlexRole(this LibraryMediaItemRoleDTO role) =>
        new()
        {
            Name = role.Tag,
            PlexKey = role.PlexId,
            Role = role.Role,
            TagKey = role.TagKey,
            Thumb = role.Thumb,
        };

    public static List<PlexRole> ToPlexRole(this List<LibraryMediaItemRoleDTO>? countryList) =>
        countryList?.Select(x => x.ToPlexRole()).ToList() ?? [];

    public static PlexCountry ToPlexCountry(this LibraryMediaItemCountryDTO country) =>
        new() { Name = country.Tag, PlexKey = country.PlexId };

    public static List<PlexCountry> ToPlexCountry(this List<LibraryMediaItemCountryDTO>? countryList) =>
        countryList?.Select(x => x.ToPlexCountry()).ToList() ?? [];

    public static PlexGenre ToPlexGenre(this LibraryMediaItemGenreDTO genre) =>
        new() { Name = genre.Tag, PlexKey = genre.PlexId };

    public static List<PlexGenre> ToPlexGenre(this List<LibraryMediaItemGenreDTO>? genreList) =>
        genreList?.Select(x => x.ToPlexGenre()).ToList() ?? [];
}
