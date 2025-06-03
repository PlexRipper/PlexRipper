namespace PlexRipper.BaseTests;

public partial class FakePlexApiData
{
    private static Faker<LibraryMediaItemCountryDTO> _libraryMediaItemCountryDTO =
        new Faker<LibraryMediaItemCountryDTO>()
            .StrictMode(true)
            .RuleFor(x => x.PlexId, _ => GetUniqueNumber())
            .RuleFor(x => x.Name, f => f.Address.Country())
            .RuleFor(x => x.Filter, (_, x) => $"country={x.PlexId}");

    public static Faker<LibraryMediaItemCountryDTO> GetLibraryMediaItemCountryDTO(
        Seed seed,
        Action<FakeDataConfig>? options = null
    ) => _libraryMediaItemCountryDTO.UseSeed(seed.Next());
}
