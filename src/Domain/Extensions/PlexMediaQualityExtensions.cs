namespace PlexRipper.Domain;

public static class PlexMediaQualityExtensions
{
    public static List<PlexMediaQuality> PickMediaQuality(this List<PlexMediaQuality> qualities)
    {
        if (!qualities.Any())
            return [];

        var nonUnknown = qualities.OrderByDescending(q => (int)q.Quality).ToList();

        return [nonUnknown.First()];
    }
}
