namespace PlexRipper.BaseTests;

public partial class FakePlexApiData
{
    private static Faker<LibraryMediaItemGenreDTO> _libraryMediaItemGenreDTO = new Faker<LibraryMediaItemGenreDTO>()
        .StrictMode(true)
        .RuleFor(x => x.PlexId, _ => GetUniqueNumber())
        .RuleFor(x => x.Name, f => f.PlexMedia().MediaGenre())
        .RuleFor(x => x.Filter, (_, x) => $"genre={x.PlexId}");

    public static Faker<LibraryMediaItemGenreDTO> GetLibraryMediaItemGenreDTO(
        Seed seed,
        Action<FakeDataConfig>? options = null
    ) => _libraryMediaItemGenreDTO.UseSeed(seed.Next());
}
