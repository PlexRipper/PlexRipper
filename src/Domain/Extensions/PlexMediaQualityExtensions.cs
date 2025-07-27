namespace PlexRipper.Domain;

public static class PlexMediaQualityExtensions
{
    public static ICollection<BasePlexMediaQuality> PickMediaQuality(this ICollection<BasePlexMediaQuality> qualities)
    {
        if (!qualities.Any())
            return [];

        var nonUnknown = qualities.SortByQuality().ToList();

        return [nonUnknown.Last()];
    }

    /// <summary>
    /// Sorts a collection of BasePlexMediaQuality objects from lowest to highest resolution.
    /// </summary>
    /// <param name="qualities">The collection of BasePlexMediaQuality instances to sort.</param>
    /// <returns>A sorted sequence of BasePlexMediaQuality instances, ordered by resolution.</returns>
    public static IEnumerable<T> SortByQuality<T>(this IEnumerable<T> qualities)
        where T : BasePlexMediaQuality => qualities.OrderBy(q => (int)q.Quality);
}
