namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    private static readonly Random _random = new();

    private static int GetUniqueId(List<int> alreadyGenerated, int seed = 0)
    {
        var rnd = new Random(seed);
        while (true)
        {
            var value = rnd.Next(1, 10000000);
            if (!alreadyGenerated.Contains(value))
            {
                alreadyGenerated.Add(value);
                return value;
            }
        }
    }
}
