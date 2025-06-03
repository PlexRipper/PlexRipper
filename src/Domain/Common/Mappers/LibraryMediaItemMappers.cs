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

    public static Dictionary<int, PlexActor> ToPlexIdDictionary(
        this IEnumerable<PlexActor> source,
        IReadOnlyCollection<LibraryMediaItemRoleDTO> plexApiRoles
    )
    {
        var resultDict = new Dictionary<int, PlexActor>();

        foreach (var actor in source)
        {
            var plexId = plexApiRoles.FirstOrDefault(x => x.TagKey == actor.Key);
            if (plexId != null)
                resultDict.Add(plexId.PlexId, actor);
        }

        return resultDict;
    }

    public static Dictionary<int, PlexCountry> ToPlexIdDictionary(
        this IEnumerable<PlexCountry> source,
        IReadOnlyCollection<LibraryMediaItemCountryDTO> plexApiCountries
    )
    {
        var resultDict = new Dictionary<int, PlexCountry>();

        foreach (var country in source)
        {
            var plexId = plexApiCountries.FirstOrDefault(x => x.Key == country.Key);
            if (plexId != null)
                resultDict.Add(plexId.PlexId, country);
        }

        return resultDict;
    }

    public static Dictionary<int, PlexGenre> ToPlexIdDictionary(
        this IEnumerable<PlexGenre> source,
        IReadOnlyCollection<LibraryMediaItemGenreDTO> plexApiGenres
    )
    {
        var resultDict = new Dictionary<int, PlexGenre>();

        foreach (var genre in source)
        {
            var plexId = plexApiGenres.FirstOrDefault(x => x.Key == genre.Key);
            if (plexId != null)
                resultDict.Add(plexId.PlexId, genre);
        }

        return resultDict;
    }
}
