using Bogus;
using Bogus.Premium;

namespace PlexRipper.BaseTests;

public static class PlexApiFaker
{
    public static PlexApi PlexApi(this Faker faker)
    {
        return ContextHelper.GetOrSet(faker, () => new PlexApi(faker));
    }
}

public class PlexApi : DataSet
{
    private readonly Faker _faker;

    public PlexApi(Faker faker)
    {
        _faker = faker;
    }

    private static readonly string[] Devices =
    {
        "Docker Container",
        "SHIELD Android TV",
        "PC",
    };

    private static readonly string[] LibraryTypes =
    {
        "movie",
        "show",
    };

    private static readonly string[] PlexVersions =
    {
        "1.30.0.6486-629d58034",
        "1.29.2.6364-6d72b0cf6",
        "1.29.1.6316-f4cdfea9c",
        "1.29.1.6313-f4cdfea9c",
        "1.29.0.6244-819d3678c",
        "1.28.2.6151-914ddd2b3",
        "1.28.2.6106-44a5bbd28",
        "1.28.1.6104-788f82488",
        "1.28.1.6092-87136b92b",
        "1.28.0.5999-97678ded3",
        "1.27.2.5929-a806c5905",
        "1.27.1.5916-6b0e31a64",
        "1.27.0.5897-3940636f2",
        "1.26.2.5797-5bd057d2b",
    };

    public string Device => _faker.Random.ArrayElement(Devices);

    public string LibraryType => _faker.Random.ArrayElement(LibraryTypes);

    public string AccessToken => _faker.Random.Guid().ToString();

    public string ClientId => _faker.Random.Guid().ToString();

    public string MachineIdentifier => _faker.Random.Guid().ToString();

    public string PlexVersion => _faker.Random.ArrayElement(PlexVersions);
}