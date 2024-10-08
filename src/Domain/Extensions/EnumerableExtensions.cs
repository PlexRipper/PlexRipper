namespace PlexRipper.Domain;

public static class EnumerableExtensions
{
    /// <summary>
    /// Source: https://stackoverflow.com/a/49847583/8205497.
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="selectChildren"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<T>? nodes, Func<T, IEnumerable<T>> selectChildren)
    {
        if (nodes == null)
            yield break;

        foreach (var node in nodes)
        {
            yield return node;

            var children = selectChildren == null ? node as IEnumerable<T> : selectChildren(node);

            if (children == null)
                continue;

            if (selectChildren != null)
            {
                foreach (var child in children.Flatten(selectChildren))
                    yield return child;
            }
        }
    }
}
