using Bogus;
using Bogus.Hollywood;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using PlexApi.Contracts;
using PlexRipper.PlexApi;

namespace PlexRipper.BaseTests;

public partial class FakePlexApiData
{
    private static readonly Faker<GetLibraryItemsPart> GetLibraryItemsPartFaker = new Faker<GetLibraryItemsPart>()
        .StrictMode(true)
        .RuleFor(l => l.Id, f => f.Random.Number(100000))
        .RuleFor(l => l.Key, f => f.Random.Uuid().ToString())
        .RuleFor(l => l.Duration, f => f.Random.Int(1))
        .RuleFor(l => l.File, f => f.Lorem.Word())
        .RuleFor(l => l.Size, f => f.Random.Int(1))
        .RuleFor(
            l => l.HasThumbnail,
            // set to null to avoid Unable to cast object of type 'System.Int64' to type 'System.String'.
            _ => null
        //f => f.Random.Bool() ? GetLibraryItemsHasThumbnail.True : GetLibraryItemsHasThumbnail.False
        )
        .RuleFor(l => l.OptimizedForStreaming, f => f.Random.Bool())
        .RuleFor(l => l.Has64bitOffsets, f => f.Random.Bool())
        .RuleFor(l => l.AudioProfile, _ => "dts")
        .RuleFor(l => l.Container, _ => "mkv")
        .RuleFor(l => l.Indexes, _ => "sd")
        .RuleFor(l => l.VideoProfile, _ => "high")
        .RuleFor(l => l.Stream, _ => []);

    private static readonly Faker<GetLibraryItemsMedia> GetLibraryItemsMedia = new Faker<GetLibraryItemsMedia>()
        .StrictMode(true)
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
        .RuleFor(l => l.HasVoiceActivity, f => f.Random.Bool())
        .RuleFor(l => l.Container, f => f.Lorem.Word())
        .RuleFor(l => l.VideoResolution, f => f.Lorem.Word())
        .RuleFor(
            l => l.OptimizedForStreaming,
            // set to null to avoid Unable to cast object of type 'System.Int64' to type 'System.String'.
            _ => null
        // f =>
        //     f.Random.Bool()
        //         ? GetLibraryItemsOptimizedForStreaming.Enable
        //         : GetLibraryItemsOptimizedForStreaming.Disable
        )
        .RuleFor(l => l.Has64bitOffsets, f => f.Random.Bool())
        .RuleFor(l => l.Part, _ => []); // Generated in FinishWith

    private static readonly Faker<GetLibraryItemsMetadata> GetLibraryItemsMetadata =
        new Faker<GetLibraryItemsMetadata>()
            .StrictMode(false)
            .RuleFor(l => l.RatingKey, f => f.Random.Number(100000).ToString())
            .RuleFor(l => l.ParentRatingKey, f => f.Random.Number(100000).ToString())
            .RuleFor(l => l.Key, (_, x) => $"/library/metadata/{x.RatingKey}")
            .RuleFor(l => l.Type, _ => GetLibraryItemsLibraryType.Movie) // Generated in FinishWith
            .RuleFor(l => l.Guid, _ => string.Empty) // Generated in FinishWith
            .RuleFor(l => l.Studio, f => f.Movies().Production())
            .RuleFor(l => l.Title, f => f.Movies().MovieTitle())
            .RuleFor(l => l.TitleSort, (_, x) => x.Title.ToLower())
            .RuleFor(l => l.ContentRating, _ => "nl/6")
            .RuleFor(l => l.Summary, f => f.Movies().MovieOverview())
            .RuleFor(l => l.Rating, f => f.Random.Double() * 10)
            .RuleFor(l => l.AudienceRating, f => f.Random.Double() * 10)
            .RuleFor(l => l.ViewOffset, f => f.Random.Int(1))
            .RuleFor(l => l.LastViewedAt, _ => 0)
            .RuleFor(l => l.Year, f => f.Random.Int(0, DateTime.Now.Year))
            .RuleFor(l => l.AddedAt, f => f.Date.Past().ToUnixLong())
            .RuleFor(l => l.UpdatedAt, f => f.Date.Recent().ToUnixLong())
            .RuleFor(l => l.Thumb, (_, x) => $"/library/metadata/{x.RatingKey}/thumb/${x.UpdatedAt}")
            .RuleFor(l => l.Art, (_, x) => $"/library/metadata/{x.RatingKey}/art/${x.UpdatedAt}")
            .RuleFor(l => l.Banner, (f, x) => $"/library/metadata/{x.RatingKey}/banner/{f.Random.Int(100000, 1000000)}")
            .RuleFor(l => l.Theme, (f, x) => $"/library/metadata/{x.RatingKey}/theme/{f.Random.Int(100000, 1000000)}")
            .RuleFor(l => l.Duration, f => f.Random.Int(1))
            // set to null to avoid serializing the entire LocalDate object, needs to be long
            .RuleFor(l => l.OriginallyAvailableAt, _ => null)
            .RuleFor(l => l.AudienceRatingImage, _ => "rottentomatoes://image.rating.upright")
            .RuleFor(l => l.Index, f => f.Random.Int(1))
            .RuleFor(l => l.LeafCount, f => f.Random.Int(1))
            .RuleFor(l => l.ViewedLeafCount, f => f.Random.Int(1))
            .RuleFor(l => l.ChildCount, f => f.Random.Int(1, 10))
            .RuleFor(l => l.ViewCount, _ => default)
            .RuleFor(l => l.SkipCount, _ => default)
            .RuleFor(l => l.LastViewedAt, _ => default)
            .RuleFor(l => l.Media, _ => []) // Generated in FinishWith
            .RuleFor(
                x => x.MediaGuid,
                f =>
                    [
                        new MediaGuid { Id = $"imdb://tt{f.Random.Number(1_000_000, 9_999_999)}" },
                        new MediaGuid { Id = $"tmdb://tt{f.Random.Number(100_000, 999_999)}" },
                        new MediaGuid { Id = $"tvdb://{f.Random.Number(10_000, 99_999)}" },
                    ]
            );

    /// <summary>
    /// Generates a fake response for the GetLibraryItemsResponse operation
    /// URL: /library/sections/{sectionKey}/{tag}
    /// </summary>
    private static readonly Faker<GetLibraryItemsMediaContainer> GetLibraryItemsMediaContainer =
        new Faker<GetLibraryItemsMediaContainer>()
            .StrictMode(true)
            .RuleFor(x => x.TotalSize, _ => -1) // Generated in FinishWith
            .RuleFor(x => x.Type, _ => null)
            .RuleFor(x => x.FieldType, _ => null)
            .RuleFor(x => x.Offset, _ => 0)
            .RuleFor(x => x.Content, _ => string.Empty)
            .RuleFor(x => x.MixedParents, f => f.Random.Bool())
            .RuleFor(x => x.Meta, _ => null)
            .RuleFor(x => x.AllowSync, f => f.Random.Bool())
            .RuleFor(x => x.Art, _ => "/:/resources/unknown-fanart.jpg")
            .RuleFor(x => x.Identifier, _ => "com.plexapp.plugins.library")
            .RuleFor(x => x.LibrarySectionID, _ => -1)
            .RuleFor(x => x.LibrarySectionTitle, _ => string.Empty)
            .RuleFor(x => x.LibrarySectionUUID, _ => string.Empty)
            .RuleFor(x => x.MediaTagPrefix, _ => "/system/bundle/media/flags/")
            .RuleFor(x => x.MediaTagVersion, f => f.Random.Number(0, 1000000000))
            .RuleFor(x => x.MediaTagPrefix, _ => "/system/bundle/media/flags/")
            .RuleFor(x => x.Thumb, _ => "/:/resources/unknown.png")
            .RuleFor(x => x.Title1, (f, _) => f.Name.FullName())
            .RuleFor(x => x.Title2, (_, x) => $"All {x.Title1}")
            .RuleFor(x => x.ViewGroup, _ => string.Empty)
            .RuleFor(x => x.Nocache, f => f.Random.Bool())
            .RuleFor(x => x.ViewMode, f => f.Random.Number(100000))
            .RuleFor(x => x.Metadata, _ => []) // Generated in FinishWith
            .RuleFor(x => x.Size, _ => -1); // Generated in FinishWith

    /// <summary>
    /// Generates a fake response for the GetLibraryItemsResponse operation
    /// URL: /library/sections/{sectionKey}/{tag}
    /// </summary>
    public static GetLibraryItemsResponseBody GetPlexLibrarySectionAllResponse(
        Seed seed,
        GetAllLibrariesDirectory library,
        int mediaCount = 0,
        Action<PlexApiDataConfig>? options = null
    )
    {
        var config = PlexApiDataConfig.FromOptions(options);
        var type = library.Type.ToPlexMediaTypeFromPlexApi();

        var totalSize = type switch
        {
            PlexMediaType.Movie => config.MoviesPerLibraryCount,
            PlexMediaType.TvShow => config.TvShowsPerLibraryCount
                * config.SeasonsPerTvShowCount
                * config.EpisodesPerSeasonCount,
            _ => throw new ArgumentOutOfRangeException(),
        };

        return new GetLibraryItemsResponseBody
        {
            MediaContainer = GetLibraryItemsMediaContainer
                .UseSeed(seed.Next())
                .FinishWith(
                    (_, x) =>
                    {
                        x.LibrarySectionID = long.Parse(library.Key);
                        x.LibrarySectionTitle = library.Title;
                        x.LibrarySectionUUID = library.Uuid;
                        x.ViewGroup = library.Type.Value();
                        x.Thumb = x.Thumb.Replace("unknown", x.ViewGroup);
                        x.Art = x.Art.Replace("unknown", x.ViewGroup);
                        x.Metadata = GetLibraryMediaMetadata(seed, type, options).Generate(mediaCount);
                        x.Size = x.Metadata!.Count;
                        x.TotalSize = totalSize;
                    }
                )
                .Generate(),
        };
    }

    public static Faker<GetLibraryItemsMetadata> GetLibraryMediaMetadata(
        Seed seed,
        PlexMediaType type,
        Action<PlexApiDataConfig>? options = null
    )
    {
        GetLibraryItemsLibraryType GetPlexMediaType() =>
            type switch
            {
                PlexMediaType.Movie => GetLibraryItemsLibraryType.Movie,
                PlexMediaType.TvShow => GetLibraryItemsLibraryType.TvShow,
                PlexMediaType.Season => GetLibraryItemsLibraryType.Season,
                PlexMediaType.Episode => GetLibraryItemsLibraryType.Episode,
                _ => throw new InvalidOperationException($"Invalid PlexMediaType: {type} value."),
            };

        return GetLibraryItemsMetadata
            .UseSeed(seed.Next())
            .FinishWith(
                (f, x) =>
                {
                    x.Type = GetPlexMediaType();
                    x.Media = [GetPlexMedium(seed, options).Generate()];
                    x.Guid = $"plex://{type.ToPlexMediaTypeString().ToLower()}/{f.Random.AlphaNumeric(24)}";
                }
            );
    }

    public static Faker<GetLibraryItemsMedia> GetPlexMedium(Seed seed, Action<PlexApiDataConfig>? options = null)
    {
        return GetLibraryItemsMedia
            .UseSeed(seed.Next())
            .FinishWith(
                (_, x) =>
                {
                    x.Part = [GetPlexPart(seed, options).Generate()];
                }
            );
    }

    public static Faker<GetLibraryItemsPart> GetPlexPart(Seed seed, Action<PlexApiDataConfig>? options = null) =>
        GetLibraryItemsPartFaker.UseSeed(seed.Next());
}
