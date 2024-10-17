using Bogus;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using PlexRipper.PlexApi;

namespace PlexRipper.BaseTests;

public partial class FakePlexApiData
{
    public static GetAllLibrariesResponseBody GetAllLibrariesResponseBody(
        Seed seed,
        Action<PlexApiDataConfig>? options = null
    )
    {
        var config = PlexApiDataConfig.FromOptions(options);

        var mediaContainer = new Faker<GetAllLibrariesMediaContainer>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.AllowSync, f => f.Random.Bool())
            .RuleFor(x => x.Title1, f => f.Company.CompanyName())
            .RuleFor(x => x.Directory, _ => GetLibrariesResponseDirectory(seed, options).Generate(config.LibraryCount))
            .RuleFor(x => x.Size, (_, res) => res.Directory.Count);

        return new Faker<GetAllLibrariesResponseBody>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.MediaContainer, _ => mediaContainer.Generate())
            .Generate();
    }

    private static Faker<GetAllLibrariesDirectory> GetLibrariesResponseDirectory(
        Seed seed,
        Action<PlexApiDataConfig>? options = null
    )
    {
        var config = PlexApiDataConfig.FromOptions(options);

        return new Faker<GetAllLibrariesDirectory>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.AllowSync, f => f.Random.Bool())
            .RuleFor(x => x.Art, _ => "/:/resources/movie-fanart.jpg")
            .RuleFor(x => x.Composite, _ => "/library/sections/7/composite/9999999")
            .RuleFor(x => x.Filters, f => f.Random.Bool())
            .RuleFor(x => x.Refreshing, f => f.Random.Bool())
            .RuleFor(x => x.Thumb, _ => "/:/resources/movie.png")
            .RuleFor(x => x.Key, _ => GetUniqueNumber().ToString())
            .RuleFor(x => x.Type, f => f.PlexApi().LibraryType)
            .RuleFor(x => x.Title, f => f.Company.CompanyName())
            .RuleFor(x => x.Agent, _ => "tv.plex.agents.movie")
            .RuleFor(x => x.Scanner, _ => "Plex Movie")
            .RuleFor(x => x.Language, _ => "en-US")
            .RuleFor(x => x.Uuid, f => f.PlexApi().ClientId)
            .RuleFor(x => x.UpdatedAt, f => f.Date.Recent().ToUnixLong())
            .RuleFor(x => x.CreatedAt, f => f.Date.Past(4).ToUnixLong())
            .RuleFor(x => x.ScannedAt, f => f.Date.Recent().ToUnixLong())
            .RuleFor(x => x.Content, f => f.Random.Bool())
            .RuleFor(x => x.Directory, f => f.Random.Bool())
            .RuleFor(x => x.ContentChangedAt, f => (int)f.Date.Recent().ToUnixLong())
            .RuleFor(x => x.Hidden, _ => 0)
            .RuleFor(
                x => x.Location,
                f => [new GetAllLibrariesLocation { Id = f.Random.Number(100000), Path = f.System.DirectoryPath() }]
            )
            .FinishWith(
                (f, directory) =>
                {
                    directory.Composite = $"/library/sections/{directory.Key}/composite/{f.Random.Number(100000)}";
                }
            );
    }
}
