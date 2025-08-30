namespace Reaparr.BaseTests;

public static class BogusFakerExtensions
{
    /// <summary>
    /// Generates exactly <paramref name="count"/> unique items,
    /// drawing up to <paramref name="batchSize"/> at a time.
    /// Throws if it can’t fill the requested count within a reasonable loop.
    /// </summary>
    public static List<T> GenerateUnique<T, TKey>(
        this Faker<T> faker,
        int count,
        Func<T, TKey> keySelector,
        int batchSize = 100
    )
        where T : class
    {
        if (count <= 0)
            return [];

        var result = new List<T>(count);
        var seen = new HashSet<TKey>(count);
        var loopGuard = 0;

        while (result.Count < count)
        {
            // decide how many to ask Faker for this round
            var toGenerate = Math.Min(batchSize, count - result.Count);

            // generate a batch
            var batch = faker.Generate(toGenerate);

            // pick out the unseen ones
            foreach (var item in batch)
            {
                var key = keySelector(item);
                if (seen.Add(key))
                {
                    result.Add(item);
                    if (result.Count == count)
                        break;
                }
            }

            // prevent infinite loops if key-space is too small
            if (++loopGuard > count * 10)
            {
                throw new InvalidOperationException(
                    "Unable to generate unique items within the specified count. "
                        + "Consider increasing batchSize, expanding the key range, or adjusting the keySelector."
                );
            }
        }

        return result;
    }

    public static string PickRandomFromDataset(this Faker faker, HashSet<string> dataSet)
    {
        if (dataSet.Count == 0)
            throw new InvalidOperationException("The dataset is empty.");

        var index = faker.Random.Int(0, dataSet.Count - 1);
        return dataSet.ElementAt(index);
    }
}
