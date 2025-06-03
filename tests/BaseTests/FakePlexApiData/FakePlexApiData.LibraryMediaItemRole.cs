namespace PlexRipper.BaseTests;

public partial class FakePlexApiData
{
    private static Faker<LibraryMediaItemRoleDTO> _libraryMediaItemRoleDTO = new Faker<LibraryMediaItemRoleDTO>()
        .StrictMode(true)
        .RuleFor(x => x.PlexId, _ => GetUniqueNumber())
        .RuleFor(x => x.Name, f => f.Name.FullName())
        .RuleFor(x => x.Role, f => f.Image.PicsumUrl())
        .RuleFor(x => x.Filter, (_, x) => $"actor={x.PlexId}")
        .RuleFor(x => x.TagKey, f => f.Random.AlphaNumeric(24))
        .RuleFor(x => x.Thumb, f => f.Image.PicsumUrl());

    public static Faker<LibraryMediaItemRoleDTO> GetLibraryMediaItemRoleDTO(
        Seed seed,
        Action<FakeDataConfig>? options = null
    ) => _libraryMediaItemRoleDTO.UseSeed(seed.Next());
}
