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

    public string Device => _faker.Random.ArrayElement(Devices);

    public string AccessToken => _faker.Random.String(40);

    public string ClientId => _faker.Random.String(40);
}