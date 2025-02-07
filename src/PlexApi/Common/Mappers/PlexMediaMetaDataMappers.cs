namespace PlexRipper.PlexApi;

public static class PlexMediaMetaDataMappers
{
    public static List<PlexCountry> ToUniquePlexCountry(this List<LibraryMediaItemDTO> list)
    {
        return list.SelectMany(x => x.Country ?? []).DistinctBy(x => x.Id).Select(x => x.ToPlexCountry()).ToList();
    }

    public static List<PlexGenre> ToUniquePlexGenre(this List<LibraryMediaItemDTO> list)
    {
        return list.SelectMany(x => x.Genre ?? []).DistinctBy(x => x.Id).Select(x => x.ToPlexGenre()).ToList();
    }

    public static List<PlexRole> ToUniquePlexRole(this List<LibraryMediaItemDTO> list)
    {
        return list.SelectMany(x => x.Role ?? []).DistinctBy(x => x.Id).Select(x => x.ToPlexRole()).ToList();
    }
}
