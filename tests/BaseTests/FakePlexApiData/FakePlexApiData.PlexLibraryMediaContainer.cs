using Bogus;
using LukeHagar.PlexAPI.SDK.Models.Requests;

namespace PlexRipper.BaseTests;

public partial class FakePlexApiData
{
    public static GetAllLibrariesResponseBody GetLibraryMediaContainer(Action<PlexApiDataConfig> options = null)
    {
        var config = PlexApiDataConfig.FromOptions(options);

        var mediaContainer = new Faker<GetAllLibrariesMediaContainer>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.Size, _ => 0)
            .RuleFor(x => x.AllowSync, f => f.Random.Bool())
            .RuleFor(x => x.Title1, f => f.Company.CompanyName())
            .RuleFor(x => x.Directory, _ => GetLibrariesResponseDirectory(options).Generate(config.LibraryCount))
            .FinishWith(
                (_, container) =>
                {
                    container.Size = container.Directory.Count;
                }
            );

        return new Faker<GetAllLibrariesResponseBody>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.MediaContainer, _ => mediaContainer.Generate())
            .Generate();
    }

    private static Faker<GetAllLibrariesDirectory> GetLibrariesResponseDirectory(
        Action<PlexApiDataConfig> options = null
    )
    {
        var config = PlexApiDataConfig.FromOptions(options);

        return new Faker<GetAllLibrariesDirectory>()
            .StrictMode(true)
            .UseSeed(config.GetSeed())
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
            .RuleFor(x => x.UpdatedAt, f => f.Date.Recent().ToBinary())
            .RuleFor(x => x.CreatedAt, f => f.Date.Past(4).ToBinary())
            .RuleFor(x => x.ScannedAt, f => f.Date.Recent().ToBinary())
            .RuleFor(x => x.Content, f => f.Random.Bool())
            .RuleFor(x => x.Directory, f => f.Random.Bool())
            .RuleFor(x => x.ContentChangedAt, f => f.Date.Recent().ToBinary())
            .RuleFor(x => x.Hidden, _ => 0)
            .RuleFor(
                x => x.Location,
                f => [new Location { Id = f.Random.Number(100000), Path = f.System.DirectoryPath() }]
            )
            .FinishWith(
                (f, directory) =>
                {
                    directory.Composite = $"/library/sections/{directory.Key}/composite/{f.Random.Number(100000)}";
                }
            );
    }
}
