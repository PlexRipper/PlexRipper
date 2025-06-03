namespace PlexRipper.Domain;

public static class LibraryMediaItemMappers
{
    public static PlexActor ToPlexActor(this LibraryMediaItemRoleDTO role) =>
        new()
        {
            Name = role.Name,
            PlexKey = role.TagKey,
            Thumb = role.Thumb,
        };

    public static List<PlexActor> ToPlexActor(this IEnumerable<LibraryMediaItemRoleDTO>? countryList) =>
        countryList?.Select(x => x.ToPlexActor()).ToList() ?? [];

    public static PlexCountry ToPlexCountry(this LibraryMediaItemCountryDTO country) =>
        new() { Name = country.Name, PlexKey = country.PlexId };

    public static List<PlexCountry> ToPlexCountry(this IEnumerable<LibraryMediaItemCountryDTO>? countryList) =>
        countryList?.Select(x => x.ToPlexCountry()).ToList() ?? [];

    public static PlexGenre ToPlexGenre(this LibraryMediaItemGenreDTO genre) =>
        new() { Name = genre.Name, PlexKey = genre.PlexId };

    public static List<PlexGenre> ToPlexGenre(this IEnumerable<LibraryMediaItemGenreDTO>? genreList) =>
        genreList?.Select(x => x.ToPlexGenre()).ToList() ?? [];
}
