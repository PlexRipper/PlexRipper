namespace PlexRipper.PlexApi;

public static class PlexMediaMetaDataMappers
{
    public static List<PlexCountry> ToPlexCountry(this List<LibraryMediaItemDTO> list)
    {
        return list.SelectMany(x => x.Country).Select(x => x.ToPlexCountry()).ToList();
    }

    public static List<PlexGenre> ToPlexGenre(this List<LibraryMediaItemDTO> list)
    {
        return list.SelectMany(x => x.Genre).Select(x => x.ToPlexGenre()).ToList();
    }

    public static List<PlexRole> ToPlexRole(this List<LibraryMediaItemDTO> list)
    {
        return list.SelectMany(x => x.Role).Select(x => x.ToPlexRole()).ToList();
    }
}
