namespace Reaparr.Domain;

public static class HashSetExtensions
{
    /// <summary>
    /// Get an item from a HashSet by index.
    /// </summary>
    /// <param name="hashSet"> The HashSet to get the item from. </param>
    /// <param name="index"> The index of the item to get. </param>
    /// <typeparam name="T"> The type of the HashSet. </typeparam>
    /// <returns> The item at the index. </returns>
    public static T GetByIndex<T>(this HashSet<T> hashSet, int index)
    {
        var currentIndex = 0;
        foreach (var item in hashSet)
        {
            if (currentIndex == index)
            {
                return item;
            }

            currentIndex++;
        }

        return hashSet.First();
    }
}
