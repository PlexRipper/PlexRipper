using Bogus;
using Bogus.Hollywood;
using PlexRipper.PlexApi.Api;
using PlexRipper.PlexApi.Models;

namespace PlexRipper.BaseTests;

public partial class FakePlexApiData
{
    public static PlexMediaContainerDTO GetPlexLibrarySectionAllResponse(LibrariesResponseDirectory library, Action<PlexApiDataConfig> options = null)
    {
        var config = PlexApiDataConfig.FromOptions(options);

        return new PlexMediaContainerDTO()
        {
            MediaContainer = new Faker<MediaContainer>()
                .StrictMode(false)
                .UseSeed(config.Seed)
                .RuleFor(x => x.Size, _ => config.LibraryMetaDataCount)
                .RuleFor(x => x.AllowSync, f => f.Random.Bool())
                .RuleFor(x => x.Art, _ => $"/:/resources/{library.Type}-fanart.jpg")
                .RuleFor(x => x.Identifier, _ => "com.plexapp.plugins.library")
                .RuleFor(x => x.LibrarySectionID, _ => Convert.ToInt32(library.Key))
                .RuleFor(x => x.LibrarySectionTitle, _ => library.Title)
                .RuleFor(x => x.LibrarySectionUUID, _ => library.Uuid)
                .RuleFor(x => x.MediaTagPrefix, _ => "/system/bundle/media/flags/")
                .RuleFor(x => x.MediaTagVersion, f => f.Random.Number(0, 1000000000))
                .RuleFor(x => x.MediaTagPrefix, _ => "/system/bundle/media/flags/")
                .RuleFor(x => x.Thumb, _ => $"/:/resources/{library.Type}.png")
                .RuleFor(x => x.Title1, _ => library.Title)
                .RuleFor(x => x.Title2, _ => $"All {library.Title}")
                .RuleFor(x => x.ViewGroup, _ => library.Type)
                .RuleFor(x => x.Nocache, f => f.Random.Bool())
                .RuleFor(x => x.ViewMode, f => f.Random.Number(100000))
                .RuleFor(x => x.Metadata, _ => GetLibraryMediaMetadata(library, options).Generate(config.LibraryMetaDataCount))
                .Generate(),
        };
    }

    private static Faker<Metadata> GetLibraryMediaMetadata(
        LibrariesResponseDirectory library,
        Action<PlexApiDataConfig> options = null)
    {
        var config = PlexApiDataConfig.FromOptions(options);

        return new Faker<Metadata>()
            .StrictMode(false)
            .UseSeed(config.Seed)
            .RuleFor(l => l.RatingKey, f => f.Random.Number(100000).ToString())
            .RuleFor(l => l.Key, _ => "")
            .RuleFor(l => l.Guid, f => $"plex://{library.Type}/{f.Random.Guid()}")
            .RuleFor(l => l.Studio, f => f.Movies().Production())
            .RuleFor(l => l.Type, _ => library.Type)
            .RuleFor(l => l.Title, f => f.Movies().MovieTitle())
            .RuleFor(l => l.TitleSort, _ => "")
            .RuleFor(l => l.ContentRating, _ => "nl/6")
            .RuleFor(l => l.Summary, f => f.Movies().MovieOverview())
            .RuleFor(l => l.Rating, f => f.Random.Double() * 10)
            .RuleFor(l => l.AudienceRating, f => f.Random.Double() * 10)
            .RuleFor(l => l.ViewOffset, f => f.Random.Int())
            .RuleFor(l => l.LastViewedAt, _ => 0)
            .RuleFor(l => l.Year, f => f.Random.Int(0, 2023))
            .RuleFor(l => l.TagLine, f => f.Movies().MovieTagline())
            .RuleFor(l => l.Thumb, _ => "/library/metadata/44909/thumb/15670134167")
            .RuleFor(l => l.Art, _ => "/library/metadata/44909/art/15670134167")
            .RuleFor(l => l.Duration, f => f.Random.Int())
            .RuleFor(l => l.OriginallyAvailableAt, f => f.Date.Past(2).ToShortDateString())
            .RuleFor(l => l.AddedAt, f => f.Date.Past(1))
            .RuleFor(l => l.UpdatedAt, f => f.Date.Recent())
            .RuleFor(l => l.AudienceRatingImage, _ => "rottentomatoes://image.rating.upright")
            .RuleFor(l => l.Index, f => f.Random.Int())
            .RuleFor(l => l.Banner, f => f.Lorem.Word())
            .RuleFor(l => l.LeafCount, f => f.Random.Int())
            .RuleFor(l => l.ViewedLeafCount, f => f.Random.Int())
            .RuleFor(l => l.ChildCount, f => f.Random.Int())
            .RuleFor(l => l.Genres, _ => default)
            .RuleFor(l => l.Roles, _ => default)
            .RuleFor(l => l.ViewCount, _ => default)
            .RuleFor(l => l.SkipCount, _ => default)
            .RuleFor(l => l.LastViewedAt, _ => default)
            .RuleFor(l => l.Theme, f => f.Lorem.Word())
            .RuleFor(l => l.FlattenSeasons, f => f.Random.Bool())
            .RuleFor(l => l.ShowOrdering, f => f.Lorem.Word())
            .FinishWith((_, metadata) =>
            {
                metadata.Key = $"/library/metadata/{metadata.Key}";
                metadata.TitleSort = metadata.TitleSort.ToLower();
            });
    }
}