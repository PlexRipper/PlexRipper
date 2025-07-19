namespace PlexRipper.Domain;

public static class LibraryMediaItemMappers
{
    public static PlexActor ToPlexActor(this LibraryMediaItemRoleDTO role) =>
        new() { Name = role.Name, Key = role.Key };

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

    public static Dictionary<string, PlexActor> ToHashKeyDictionary(
        this IEnumerable<PlexActor> source,
        IReadOnlyCollection<LibraryMediaItemRoleDTO> plexApiRoles
    )
    {
        var resultDict = new Dictionary<string, PlexActor>();

        foreach (var actor in source)
        {
            var plexId = plexApiRoles.FirstOrDefault(x => x.Key == actor.Key);
            if (plexId != null)
                resultDict.Add(plexId.Key, actor);
        }

        return resultDict;
    }

    public static Dictionary<string, PlexCountry> ToHashKeyDictionary(
        this IEnumerable<PlexCountry> source,
        IReadOnlyCollection<LibraryMediaItemCountryDTO> plexApiCountries
    )
    {
        var resultDict = new Dictionary<string, PlexCountry>();

        foreach (var country in source)
        {
            var plexId = plexApiCountries.FirstOrDefault(x => x.Key == country.Key);
            if (plexId != null)
                resultDict.Add(plexId.Key, country);
        }

        return resultDict;
    }

    public static Dictionary<string, PlexGenre> ToHashKeyDictionary(
        this IEnumerable<PlexGenre> source,
        IReadOnlyCollection<LibraryMediaItemGenreDTO> plexApiGenres
    )
    {
        var resultDict = new Dictionary<string, PlexGenre>();

        foreach (var genre in source)
        {
            var plexId = plexApiGenres.FirstOrDefault(x => x.Key == genre.Key);
            if (plexId != null)
                resultDict.Add(plexId.Key, genre);
        }

        return resultDict;
    }
}
