using Bogus;
using Bogus.Hollywood;
using PlexRipper.PlexApi.Api;
using PlexRipper.PlexApi.Models;
using Stream = PlexRipper.PlexApi.Models.Stream;

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
                .RuleFor(x => x.Metadata, _ => GetLibraryMediaMetadata(library.Type.ToPlexMediaType(), options).Generate(config.LibraryMetaDataCount))
                .Generate(),
        };
    }

    public static Faker<Metadata> GetLibraryMediaMetadata(
        PlexMediaType type,
        Action<PlexApiDataConfig> options = null)
    {
        var config = PlexApiDataConfig.FromOptions(options);

        return new Faker<Metadata>()
            .StrictMode(false)
            .UseSeed(config.Seed)
            .RuleFor(l => l.RatingKey, f => f.Random.Number(100000).ToString())
            .RuleFor(l => l.Key, _ => "")
            .RuleFor(l => l.Guid, f => $"plex://{type.ToPlexMediaTypeString().ToLower()}/{f.Random.Guid()}")
            .RuleFor(l => l.Studio, f => f.Movies().Production())
            .RuleFor(l => l.Type, _ => type.ToPlexMediaTypeString().ToLower())
            .RuleFor(l => l.Title, f => f.Movies().MovieTitle())
            .RuleFor(l => l.TitleSort, _ => "")
            .RuleFor(l => l.ContentRating, _ => "nl/6")
            .RuleFor(l => l.Summary, f => f.Movies().MovieOverview())
            .RuleFor(l => l.Rating, f => f.Random.Double() * 10)
            .RuleFor(l => l.AudienceRating, f => f.Random.Double() * 10)
            .RuleFor(l => l.ViewOffset, f => f.Random.Int(1))
            .RuleFor(l => l.LibrarySectionTitle, f => f.Company.CompanyName())
            .RuleFor(l => l.LibrarySectionId, f => f.Random.Int(1))
            .RuleFor(l => l.LibrarySectionKey, f => f.Random.Uuid().ToString())
            .RuleFor(l => l.LastViewedAt, _ => 0)
            .RuleFor(l => l.Year, f => f.Random.Int(0, 2023))
            .RuleFor(l => l.TagLine, f => f.Movies().MovieTagline())
            .RuleFor(l => l.Thumb, __ => "")
            .RuleFor(l => l.Banner, _ => "")
            .RuleFor(l => l.Theme, _ => "")
            .RuleFor(l => l.Art, _ => "")
            .RuleFor(l => l.Duration, f => f.Random.Int(1))
            .RuleFor(l => l.OriginallyAvailableAt, f => f.Date.Past(2).ToShortDateString())
            .RuleFor(l => l.AddedAt, f => f.Date.Past(1))
            .RuleFor(l => l.UpdatedAt, f => f.Date.Recent())
            .RuleFor(l => l.AudienceRatingImage, _ => "rottentomatoes://image.rating.upright")
            .RuleFor(l => l.Index, f => f.Random.Int(1))
            .RuleFor(l => l.LeafCount, f => f.Random.Int(1))
            .RuleFor(l => l.ViewedLeafCount, f => f.Random.Int(1))
            .RuleFor(l => l.ChildCount, f => f.Random.Int(1))
            .RuleFor(l => l.Genres, _ => default)
            .RuleFor(l => l.Roles, _ => default)
            .RuleFor(l => l.ViewCount, _ => default)
            .RuleFor(l => l.SkipCount, _ => default)
            .RuleFor(l => l.LastViewedAt, _ => default)
            .RuleFor(l => l.Media, _ => new List<Medium>() { GetPlexMedium(options).Generate() })
            .RuleFor(l => l.FlattenSeasons, f => f.Random.Bool())
            .RuleFor(l => l.ShowOrdering, f => f.Lorem.Word())
            .FinishWith((f, metadata) =>
            {
                var metaDataKey = f.Random.Int(1);
                metadata.Thumb = $"/library/metadata/{metadata.Key}/thumb/{metaDataKey}";
                metadata.Art = $"/library/metadata/{metadata.Key}/art/{metaDataKey}";
                metadata.Theme = $"/library/metadata/{metadata.Key}/theme/{metaDataKey}";
                metadata.Banner = $"/library/metadata/{metadata.Key}/banner/{metaDataKey}";

                metadata.Key = $"/library/metadata/{metadata.Key}";
                metadata.TitleSort = metadata.TitleSort.ToLower();
            });
    }

    public static Faker<Medium> GetPlexMedium(
        Action<PlexApiDataConfig> options = null)
    {
        var config = PlexApiDataConfig.FromOptions(options);

        return new Faker<Medium>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(l => l.Id, f => f.Random.Number(100000))
            .RuleFor(l => l.Duration, f => f.Random.Int(1))
            .RuleFor(l => l.Bitrate, f => f.Random.Int(1))
            .RuleFor(l => l.Width, f => f.Random.Int(1))
            .RuleFor(l => l.Height, f => f.Random.Int(1))
            .RuleFor(l => l.AspectRatio, f => f.Random.Double())
            .RuleFor(l => l.AudioChannels, f => f.Random.Int(1))
            .RuleFor(l => l.AudioCodec, f => f.Lorem.Word())
            .RuleFor(l => l.VideoCodec, f => f.Lorem.Word())
            .RuleFor(l => l.Container, f => f.Lorem.Word())
            .RuleFor(l => l.VideoFrameRate, _ => "24p")
            .RuleFor(l => l.AudioProfile, _ => "dts")
            .RuleFor(l => l.VideoProfile, _ => "high")
            .RuleFor(l => l.OptimizedForStreaming, f => f.Random.Bool() ? 1 : 0)
            .RuleFor(l => l.Container, f => f.Lorem.Word())
            .RuleFor(l => l.Protocol, f => f.Lorem.Word())
            .RuleFor(l => l.VideoResolution, f => f.Lorem.Word())
            .RuleFor(l => l.Selected, f => f.Random.Bool())
            .RuleFor(l => l.Part, _ => new[] { GetPlexPart(options).Generate() });
    }

    public static Faker<Part> GetPlexPart(
        Action<PlexApiDataConfig> options = null)
    {
        var config = PlexApiDataConfig.FromOptions(options);

        return new Faker<Part>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(l => l.Id, f => f.Random.Number(100000))
            .RuleFor(l => l.Key, f => f.Random.Uuid().ToString())
            .RuleFor(l => l.Duration, f => f.Random.Int(1))
            .RuleFor(l => l.File, f => f.Lorem.Word())
            .RuleFor(l => l.Size, f => f.Random.Int(1))
            .RuleFor(l => l.Stream, _ => new Stream[] { })
            .RuleFor(l => l.HasChapterTextStream, f => f.Random.Bool())
            .RuleFor(l => l.HasThumbnail, f => f.Random.Bool().ToString())
            .RuleFor(l => l.AudioProfile, _ => "dts")
            .RuleFor(l => l.Container, _ => "mkv")
            .RuleFor(l => l.Indexes, _ => "sd")
            .RuleFor(l => l.VideoProfile, _ => "high");
    }
}