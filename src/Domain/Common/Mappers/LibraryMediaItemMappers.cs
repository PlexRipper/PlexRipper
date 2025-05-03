namespace PlexRipper.Domain;

public static class LibraryMediaItemMappers
{
    public static PlexRole ToPlexRole(this LibraryMediaItemRoleDTO role) => new() { Name = role.Tag };

    public static List<PlexRole> ToPlexRole(this List<LibraryMediaItemRoleDTO>? countryList) =>
        countryList?.Select(x => x.ToPlexRole()).ToList() ?? [];

    public static PlexCountry ToPlexCountry(this MetaDataCountryDTO country) => new() { Name = country.Tag };

    public static List<PlexCountry> ToPlexCountry(this List<MetaDataCountryDTO>? countryList) =>
        countryList?.Select(x => x.ToPlexCountry()).ToList() ?? [];

    public static PlexGenre ToPlexGenre(this LibraryMediaItemGenreDTO genre) => new() { Name = genre.Tag };

    public static List<PlexGenre> ToPlexGenre(this List<LibraryMediaItemGenreDTO>? genreList) =>
        genreList?.Select(x => x.ToPlexGenre()).ToList() ?? [];
}
