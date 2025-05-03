using System.Collections.Concurrent;
using Bogus;

namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    private static readonly Random RandomInstance = new();

    private static readonly HashSet<int> AlreadyGenerated = [0];

    private static readonly ConcurrentDictionary<Type, object> _fakerDictionary = new();

    private static string DownloadFileUrl => "/library/parts/653125/119385313456/file.mp4";

    private static int GetUniqueNumber()
    {
        if (AlreadyGenerated.Count >= int.MaxValue)
            throw new InvalidOperationException("All possible unique numbers have been generated.");

        var value = 0;
        while (AlreadyGenerated.Contains(value))
        {
            value = RandomInstance.Next(1, int.MaxValue);
        }

        AlreadyGenerated.Add(value);
        return value;
    }

    private static Faker<T> GetOrCreateCachedFaker<T>(Func<Faker<T>> factory)
        where T : class
    {
        return ((Faker<T>)_fakerDictionary.GetOrAdd(typeof(T), _ => factory())).Clone();
    }
}
