namespace PlexRipper.BaseTests;

public static partial class FakePlexApiData
{
    private static readonly Random _random = new();

    private static int GetUniqueId(List<int> alreadyGenerated, Action<UnitTestDataConfig> options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        var rnd = new Random(config.Seed);
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