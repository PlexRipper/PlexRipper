namespace Reaparr.Domain;

public static class PlexGenreTypeExtensions
{
    private static readonly IReadOnlyDictionary<string, PlexGenreType> _genreTypes = new Dictionary<
        string,
        PlexGenreType
    >(StringComparer.OrdinalIgnoreCase)
    {
        // Action
        ["Action"] = PlexGenreType.Action,
        ["Martial Arts"] = PlexGenreType.Action,
        ["Superhero"] = PlexGenreType.Action,

        // Adventure
        ["Adventure"] = PlexGenreType.Adventure,
        ["Action & Adventure"] = PlexGenreType.Adventure,
        ["Action &Adventure"] = PlexGenreType.Adventure,
        ["Action/Adventure"] = PlexGenreType.Adventure,

        // Anime
        ["Anime"] = PlexGenreType.Anime,
        ["Manga"] = PlexGenreType.Anime,
        ["Animated"] = PlexGenreType.Anime,
        ["Animatie"] = PlexGenreType.Anime,
        ["Animation"] = PlexGenreType.Anime,
        ["Bishounen"] = PlexGenreType.Anime,

        // Comedy
        ["Comedy"] = PlexGenreType.Comedy,
        ["Komedi"] = PlexGenreType.Comedy,
        ["Komedie"] = PlexGenreType.Comedy,

        // Crime
        ["Crime"] = PlexGenreType.Crime,
        ["Misdaad"] = PlexGenreType.Crime,
        ["Heist"] = PlexGenreType.Crime,

        // Documentary
        ["Documentary"] = PlexGenreType.Documentary,
        ["Documentaire"] = PlexGenreType.Documentary,
        ["Biography"] = PlexGenreType.Documentary,
        ["Biographical"] = PlexGenreType.Documentary,

        // Drama
        ["Drama"] = PlexGenreType.Drama,
        ["Soap"] = PlexGenreType.Drama,
        ["Angst"] = PlexGenreType.Drama,

        // Family
        ["Family"] = PlexGenreType.Family,
        ["Familie"] = PlexGenreType.Family,
        ["Kids & Family"] = PlexGenreType.Family,
        ["Kinderen en familie"] = PlexGenreType.Family,
        ["Disney"] = PlexGenreType.Family,

        // Children
        ["Children"] = PlexGenreType.Children,
        ["Children's"] = PlexGenreType.Children,
        ["Childrens"] = PlexGenreType.Children,

        // Fantasy
        ["Fantasy"] = PlexGenreType.Fantasy,
        ["Fantasie"] = PlexGenreType.Fantasy,
        ["Contemporary fantasy"] = PlexGenreType.Fantasy,
        ["Dark fantasy"] = PlexGenreType.Fantasy,
        ["Demon"] = PlexGenreType.Fantasy,
        ["Demon world"] = PlexGenreType.Fantasy,
        ["Elf"] = PlexGenreType.Fantasy,
        ["Alternative past"] = PlexGenreType.Fantasy,

        // History
        ["History"] = PlexGenreType.History,
        ["Historisch"] = PlexGenreType.History,

        // Horror
        ["Horror"] = PlexGenreType.Horror,
        ["Suspense"] = PlexGenreType.Horror,

        // Music
        ["Music"] = PlexGenreType.Music,
        ["Musical"] = PlexGenreType.Music,
        ["Muziek"] = PlexGenreType.Music,
        ["Performances & Events"] = PlexGenreType.Music,
        ["Books & Spoken"] = PlexGenreType.Music,

        // Mystery
        ["Mystery"] = PlexGenreType.Mystery,
        ["Mysterie"] = PlexGenreType.Mystery,

        // News
        ["News"] = PlexGenreType.News,

        // Reality
        ["Reality"] = PlexGenreType.Reality,
        ["Competition reality"] = PlexGenreType.Reality,
        ["Housewives"] = PlexGenreType.Reality,

        // Romance
        ["Romance"] = PlexGenreType.Romance,
        ["Romantic"] = PlexGenreType.Romance,
        ["Romantiek"] = PlexGenreType.Romance,

        // Science fiction
        ["Sci-Fi"] = PlexGenreType.ScienceFiction,
        ["Science Fiction"] = PlexGenreType.ScienceFiction,
        ["Sciencefiction"] = PlexGenreType.ScienceFiction,
        ["Sci-Fi & Fantasy"] = PlexGenreType.ScienceFiction,
        ["Science Fiction & Fantasy"] = PlexGenreType.ScienceFiction,

        // Sport
        ["Sport"] = PlexGenreType.Sport,
        ["Sports"] = PlexGenreType.Sport,
        ["Sportcommentaar"] = PlexGenreType.Sport,
        ["Voetbal"] = PlexGenreType.Sport,

        // Thriller
        ["Thriller"] = PlexGenreType.Thriller,
        ["Spy"] = PlexGenreType.Thriller,
        ["Blackmail"] = PlexGenreType.Thriller,

        // War
        ["War"] = PlexGenreType.War,
        ["Oorlog"] = PlexGenreType.War,
        ["Military"] = PlexGenreType.War,
        ["Military & War"] = PlexGenreType.War,
        ["War & Politics"] = PlexGenreType.War,

        // Western
        ["Western"] = PlexGenreType.Western,
        ["Western<br><br><br>"] = PlexGenreType.Western,

        // Adult
        ["18 restricted"] = PlexGenreType.Adult,
        ["Adult"] = PlexGenreType.Adult,
        ["Ahegao"] = PlexGenreType.Adult,
        ["Anal"] = PlexGenreType.Adult,
        ["Bdsm"] = PlexGenreType.Adult,
        ["Cg collection"] = PlexGenreType.Adult,
        ["Chikan"] = PlexGenreType.Adult,
        ["Creampie"] = PlexGenreType.Adult,
        ["Deflowering"] = PlexGenreType.Adult,
        ["Erotic game"] = PlexGenreType.Adult,
        ["Female student"] = PlexGenreType.Adult,
        ["Female teacher"] = PlexGenreType.Adult,
        ["Harem"] = PlexGenreType.Adult,
        ["High school"] = PlexGenreType.Adult,
        ["Large breasts"] = PlexGenreType.Adult,
        ["Maid"] = PlexGenreType.Adult,
        ["Nudity"] = PlexGenreType.Adult,
        ["Sex"] = PlexGenreType.Adult,
        ["Teen"] = PlexGenreType.Adult,

        // Educational
        ["Courses"] = PlexGenreType.Educational,
        ["Earth"] = PlexGenreType.Educational,
        ["Understanding Gravity"] = PlexGenreType.Educational,
        ["Black Holes, Tides, and Curved Spacetime"] = PlexGenreType.Educational,
        ["Scripture and Lesson Support"] = PlexGenreType.Educational,

        // Entertainment
        ["Entertainment"] = PlexGenreType.Entertainment,
        ["Awards Show"] = PlexGenreType.Entertainment,
        ["Indie"] = PlexGenreType.Entertainment,

        // Game shows
        ["Game Show"] = PlexGenreType.GameShow,

        // Talk shows
        ["Talk"] = PlexGenreType.TalkShow,
        ["Talk Show"] = PlexGenreType.TalkShow,

        // TV movies
        ["TV Film"] = PlexGenreType.TVMovie,
        ["TV Movie"] = PlexGenreType.TVMovie,

        // Special interest / formats that are not really genres.
        ["Food"] = PlexGenreType.SpecialInterest,
        ["Holiday"] = PlexGenreType.SpecialInterest,
        ["Home and Garden"] = PlexGenreType.SpecialInterest,
        ["Mini-Series"] = PlexGenreType.SpecialInterest,
        ["Mormon channel"] = PlexGenreType.SpecialInterest,
        ["MyDVD"] = PlexGenreType.SpecialInterest,
        ["Podcast"] = PlexGenreType.SpecialInterest,
        ["Short"] = PlexGenreType.SpecialInterest,
        ["Special Interest"] = PlexGenreType.SpecialInterest,
        ["Travel"] = PlexGenreType.SpecialInterest,
        ["Tv"] = PlexGenreType.SpecialInterest,
        ["Video"] = PlexGenreType.SpecialInterest,
        ["Half-Length episodes"] = PlexGenreType.SpecialInterest,

        // Regional labels.
        ["Asia"] = PlexGenreType.Foreign,
        ["Americas"] = PlexGenreType.Foreign,
        ["Barat"] = PlexGenreType.Foreign,
    };

    public static PlexGenreType ToPlexGenreType(this string genre)
    {
        var normalizedGenre = genre.Trim();
        if (_genreTypes.TryGetValue(normalizedGenre, out var type))
            return type;

        var types = normalizedGenre
            .Split([',', '/', '&', ';'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(Map)
            .Where(x => x != PlexGenreType.Unknown)
            .Distinct()
            .Take(2)
            .ToList();

        return types.Count switch
        {
            0 => PlexGenreType.Unknown,
            1 => types[0],
            _ => PlexGenreType.Group,
        };
    }

    private static PlexGenreType Map(string genre) => _genreTypes.GetValueOrDefault(genre, PlexGenreType.Unknown);
}
