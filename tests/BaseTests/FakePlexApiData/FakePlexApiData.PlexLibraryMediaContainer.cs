using LukeHagar.PlexAPI.SDK.Models.Components;
using Reaparr.PlexApi;

namespace Reaparr.BaseTests;

public partial class FakePlexApiData
{
    public static Faker<LibrarySection> GetLibrariesResponseDirectory(Seed seed, PlexMediaType type)
    {
        return new Faker<LibrarySection>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.AllowSync, f => f.Random.Bool())
            .RuleFor(x => x.Art, _ => "/:/resources/movie-fanart.jpg")
            .RuleFor(x => x.Key, f => f.Random.Number(int.MaxValue).ToString())
            .RuleFor(x => x.Composite, (f, x) => $"/library/sections/{x.Key}/composite/{f.Random.Number(100000)}")
            .RuleFor(x => x.Filters, f => f.Random.Bool())
            .RuleFor(x => x.Refreshing, f => f.Random.Bool())
            .RuleFor(x => x.Thumb, _ => "/:/resources/movie.png")
            .RuleFor(x => x.Type, _ => type.ToMediaTypeString())
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
            .RuleFor(x => x.Hidden, f => f.Random.Bool())
            .RuleFor(
                x => x.Location,
                f => [new LibrarySectionLocation { Id = f.Random.Long(100000), Path = f.System.DirectoryPath() }]
            );
    }
}
