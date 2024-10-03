using NaturalSort.Extension;

namespace PlexRipper.Domain;

// Source: https://github.com/tompazourek/NaturalSort.Extension
public static class OrderByNaturalExtensions
{
    public static IEnumerable<T> OrderByNatural<T>(this IEnumerable<T> objects, Func<T, string> func) =>
        objects.OrderBy(func, StringComparison.OrdinalIgnoreCase.WithNaturalSort());

    public static IEnumerable<T> OrderByNaturalDesc<T>(this IEnumerable<T> objects, Func<T, string> func) =>
        objects.OrderByNatural(func).Reverse();

    public static string CollationName => "NATURALSORT";
}