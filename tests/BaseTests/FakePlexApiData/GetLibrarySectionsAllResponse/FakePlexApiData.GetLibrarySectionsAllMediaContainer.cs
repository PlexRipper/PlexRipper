using Bogus.Hollywood;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using NodaTime;
using Reaparr.PlexApi;

namespace Reaparr.BaseTests;

public partial class FakePlexApiData
{
    private static readonly Faker<GetLibrarySectionsAllMediaContainer> _getLibrarySectionsAllMediaContainer =
        new Faker<GetLibrarySectionsAllMediaContainer>()
            .StrictMode(true)
            .RuleFor(x => x.AllowSync, f => f.Random.Bool())
            .RuleFor(x => x.Identifier, _ => "com.plexapp.plugins.library")
            .Ignore(x => x.LibrarySectionID) // Generated in FinishWith
            .Ignore(x => x.LibrarySectionTitle) // Generated in FinishWith
            .Ignore(x => x.LibrarySectionUUID) // Generated in FinishWith
            .RuleFor(x => x.MediaTagPrefix, _ => "/system/bundle/media/flags/")
            .RuleFor(x => x.MediaTagVersion, f => f.Random.Number(0, 1000000000))
            .RuleFor(x => x.MediaTagPrefix, _ => "/system/bundle/media/flags/")
            .Ignore(x => x.Metadata) // Generated in FinishWith
            .Ignore(x => x.Size) // Generated in FinishWith
            .Ignore(x => x.TotalSize) // Generated in FinishWith
            .RuleFor(x => x.Offset, f => f.Random.Int(0, 100))
            .RuleFor(x => x.Art, f => f.Image.PicsumUrl())
            .RuleFor(x => x.Content, f => f.Lorem.Sentence())
            .RuleFor(x => x.Thumb, f => f.Image.PicsumUrl())
            .RuleFor(x => x.Nocache, f => f.Random.Bool())
            .RuleFor(x => x.Title1, f => f.Lorem.Word())
            .RuleFor(x => x.Title2, f => f.Lorem.Word())
            .RuleFor(x => x.ViewGroup, f => f.PickRandom<string>("primary", "secondary", "recentlyAdded"))
            .Ignore(x => x.Meta);

    private static readonly Faker<GetLibrarySectionsAllMetadata> _getLibrarySectionsAllMetadata =
        new Faker<GetLibrarySectionsAllMetadata>()
            .StrictMode(false)
            .RuleFor(l => l.RatingKey, f => f.Random.Number(100000).ToString())
            .RuleFor(l => l.ParentRatingKey, f => f.Random.Number(100000).ToString())
            .RuleFor(l => l.Key, (_, x) => $"/library/metadata/{x.RatingKey}")
            .RuleFor(l => l.Type, _ => GetLibrarySectionsAllLibraryType.Movie) // Generated in FinishWith
            .RuleFor(l => l.Guid, _ => string.Empty) // Generated in FinishWith
            .RuleFor(l => l.Studio, f => f.Movies().Production())
            .RuleFor(l => l.Title, f => f.Movies().MovieTitle())
            .RuleFor(l => l.TitleSort, (_, x) => x.Title.ToLower())
            .RuleFor(l => l.ContentRating, _ => "nl/6")
            .RuleFor(l => l.Summary, f => f.Movies().MovieOverview())
            .RuleFor(l => l.Rating, f => f.Random.Float() * 10)
            .RuleFor(l => l.AudienceRating, f => f.Random.Float() * 10)
            .RuleFor(l => l.ViewOffset, f => f.Random.Int(1))
            .RuleFor(l => l.LastViewedAt, _ => 0)
            .RuleFor(l => l.Year, f => f.Random.Int(0, DateTime.Now.Year))
            .RuleFor(l => l.AddedAt, f => f.Date.Past().ToUnixLong())
            .RuleFor(l => l.UpdatedAt, f => f.Date.Recent().ToUnixLong())
            .RuleFor(l => l.Thumb, (_, x) => $"/library/metadata/{x.RatingKey}/thumb/${x.UpdatedAt}")
            .RuleFor(l => l.Art, (_, x) => $"/library/metadata/{x.RatingKey}/art/${x.UpdatedAt}")
            .RuleFor(l => l.Theme, (f, x) => $"/library/metadata/{x.RatingKey}/theme/{f.Random.Int(100000, 1000000)}")
            .RuleFor(l => l.Duration, f => f.Random.Int(1))
            .RuleFor(l => l.OriginallyAvailableAt, _ => LocalDate.MinIsoValue)
            .RuleFor(l => l.AudienceRatingImage, _ => "rottentomatoes://image.rating.upright")
            .RuleFor(l => l.Index, f => f.Random.Int(1))
            .RuleFor(l => l.LeafCount, f => f.Random.Int(1))
            .RuleFor(l => l.ViewedLeafCount, f => f.Random.Int(1))
            .RuleFor(l => l.ChildCount, f => f.Random.Int(1, 10))
            .RuleFor(l => l.ViewCount, _ => default)
            .RuleFor(l => l.SkipCount, _ => default)
            .RuleFor(l => l.LastViewedAt, _ => default)
            .RuleFor(l => l.Media, _ => []) // Generated in FinishWith
            .RuleFor(l => l.Role, _ => _getLibrarySectionsAllRole!.Generate(5))
            .RuleFor(l => l.Genre, _ => _getLibrarySectionsAllGenre!.Generate(2))
            .RuleFor(l => l.Country, _ => _getLibrarySectionsAllCountry!.Generate(2))
            .RuleFor(
                x => x.Guids,
                f =>
                    [
                        new GetLibrarySectionsAllGuids { Id = $"imdb://tt{f.Random.Number(1_000_000, 9_999_999)}" },
                        new GetLibrarySectionsAllGuids { Id = $"tmdb://tt{f.Random.Number(100_000, 999_999)}" },
                        new GetLibrarySectionsAllGuids { Id = $"tvdb://{f.Random.Number(10_000, 99_999)}" },
                    ]
            );

    private static readonly Faker<GetLibrarySectionsAllRole> _getLibrarySectionsAllRole =
        new Faker<GetLibrarySectionsAllRole>().StrictMode(true).RuleFor(x => x.Tag, f => f.Movies().ActorName());

    private static readonly Faker<GetLibrarySectionsAllGenre> _getLibrarySectionsAllGenre =
        new Faker<GetLibrarySectionsAllGenre>().StrictMode(true).RuleFor(x => x.Tag, f => f.PlexMedia().MediaGenre());

    private static readonly Faker<GetLibrarySectionsAllCountry> _getLibrarySectionsAllCountry =
        new Faker<GetLibrarySectionsAllCountry>().StrictMode(true).RuleFor(x => x.Tag, f => f.Address.Country());

    private static readonly Faker<GetLibrarySectionsAllMedia> _getLibrarySectionsAllMedia =
        new Faker<GetLibrarySectionsAllMedia>()
            .StrictMode(true)
            .RuleFor(l => l.Id, f => f.Random.Number(100000))
            .RuleFor(l => l.Duration, f => f.Random.Int(1))
            .RuleFor(l => l.Bitrate, f => f.Random.Int(1))
            .RuleFor(l => l.Width, f => f.Random.Int(1))
            .RuleFor(l => l.Height, f => f.Random.Int(1))
            .RuleFor(l => l.AspectRatio, f => f.Random.Float())
            .RuleFor(l => l.AudioChannels, f => f.Random.Int(1))
            .RuleFor(l => l.AudioCodec, f => f.Lorem.Word())
            .RuleFor(l => l.VideoCodec, f => f.Lorem.Word())
            .RuleFor(l => l.Container, f => f.Lorem.Word())
            .RuleFor(l => l.VideoFrameRate, _ => "24p")
            .RuleFor(l => l.DisplayOffset, _ => 2)
            .RuleFor(l => l.AudioProfile, _ => "dts")
            .RuleFor(l => l.VideoProfile, _ => "high")
            .RuleFor(l => l.HasVoiceActivity, f => f.Random.Bool())
            .RuleFor(l => l.Container, f => f.Lorem.Word())
            .RuleFor(l => l.VideoResolution, f => f.Lorem.Word())
            .RuleFor(
                l => l.OptimizedForStreaming,
                // set to null to avoid Unable to cast object of type 'System.Int64' to type 'System.String'.
                _ => null
            )
            .RuleFor(l => l.Has64bitOffsets, f => f.Random.Bool())
            .RuleFor(l => l.Part, _ => []); // Generated in FinishWith

    private static readonly Faker<GetLibrarySectionsAllPart> _getLibrarySectionsAllPartFaker =
        new Faker<GetLibrarySectionsAllPart>()
            .StrictMode(true)
            .RuleFor(l => l.Id, f => f.Random.Number(100000))
            .RuleFor(l => l.Key, f => f.Random.Uuid().ToString())
            .RuleFor(l => l.Duration, f => f.Random.Int(1))
            .RuleFor(l => l.File, f => f.Lorem.Word())
            .RuleFor(l => l.Accessible, _ => true)
            .RuleFor(l => l.Exists, _ => true)
            .RuleFor(l => l.PacketLength, _ => 40)
            .RuleFor(l => l.Size, f => f.Random.Int(1))
            .RuleFor(
                l => l.HasThumbnail,
                // set to null to avoid Unable to cast object of type 'System.Int64' to type 'System.String'.
                _ => null
            )
            .RuleFor(
                l => l.OptimizedForStreaming,
                _ => GetLibrarySectionsAllLibraryOptimizedForStreaming.CreateBoolean(true)
            )
            .RuleFor(l => l.Has64bitOffsets, f => f.Random.Bool())
            .RuleFor(l => l.AudioProfile, _ => "dts")
            .RuleFor(l => l.Container, _ => "mkv")
            .RuleFor(l => l.Indexes, _ => "sd")
            .RuleFor(l => l.VideoProfile, _ => "high")
            .RuleFor(l => l.Stream, _ => []);

    private static readonly Faker<GetLibrarySectionsAllStream> _getLibrarySectionsAllStreamFaker =
        new Faker<GetLibrarySectionsAllStream>()
            .StrictMode(true)
            .RuleFor(x => x.Id, f => f.Random.Long(1))
            .RuleFor(x => x.StreamType, f => f.Random.Int(1, 3)) // 1: Video, 2: Audio, 3: Subtitle
            .RuleFor(x => x.Format, f => f.System.CommonFileExt())
            .RuleFor(x => x.Default, f => f.Random.Bool())
            .RuleFor(x => x.Codec, f => f.Random.Word())
            .RuleFor(x => x.Index, f => f.Random.Int(0, 10))
            .RuleFor(x => x.Bitrate, f => f.Random.Int(64_000, 320_000))
            .RuleFor(x => x.Language, f => f.Language().LanguageName())
            .RuleFor(x => x.LanguageTag, f => f.Language().LanguageTag())
            .RuleFor(x => x.LanguageCode, f => f.Language().LanguageCode())
            .RuleFor(x => x.HeaderCompression, f => f.Random.Bool())
            .RuleFor(x => x.DOVIBLCompatID, f => f.Random.Int(0, 5))
            .RuleFor(x => x.DOVIBLPresent, f => f.Random.Bool())
            .RuleFor(x => x.DOVIELPresent, f => f.Random.Bool())
            .RuleFor(x => x.DOVILevel, f => f.Random.Int(0, 10))
            .RuleFor(x => x.DOVIPresent, f => f.Random.Bool())
            .RuleFor(x => x.DOVIProfile, f => f.Random.Int(0, 10))
            .RuleFor(x => x.DOVIRPUPresent, f => f.Random.Bool())
            .RuleFor(x => x.DOVIVersion, f => f.System.Semver())
            .RuleFor(x => x.BitDepth, f => f.Random.Int(8, 12))
            .RuleFor(x => x.ChromaLocation, f => f.PickRandom("left", "center", "topleft"))
            .RuleFor(x => x.ChromaSubsampling, f => f.PickRandom("4:2:0", "4:2:2", "4:4:4"))
            .RuleFor(x => x.CodedHeight, f => f.Random.Int(240, 2160))
            .RuleFor(x => x.CodedWidth, f => f.Random.Int(320, 3840))
            .RuleFor(x => x.ClosedCaptions, f => f.Random.Bool())
            .RuleFor(x => x.ColorPrimaries, f => f.PickRandom("bt709", "bt2020", "smpte"))
            .RuleFor(x => x.ColorRange, f => f.PickRandom("tv", "pc"))
            .RuleFor(x => x.ColorSpace, f => f.PickRandom("bt709", "bt601", "bt2020"))
            .RuleFor(x => x.ColorTrc, f => f.PickRandom("smpte2084", "arib-std-b67"))
            .RuleFor(x => x.FrameRate, f => f.Random.Float(23.976f, 60f))
            .RuleFor(x => x.Key, f => f.Random.Guid().ToString())
            .RuleFor(x => x.Height, f => f.Random.Int(240, 2160))
            .RuleFor(x => x.Level, f => f.Random.Int(1, 10))
            .RuleFor(x => x.Original, f => f.Random.Bool())
            .RuleFor(x => x.HasScalingMatrix, f => f.Random.Bool())
            .RuleFor(x => x.Profile, f => f.PickRandom("main", "high", "baseline"))
            .RuleFor(x => x.ScanType, f => f.PickRandom("progressive", "interlaced"))
            .RuleFor(x => x.EmbeddedInVideo, f => f.Random.Bool().ToString().ToLower())
            .RuleFor(x => x.RefFrames, f => f.Random.Int(1, 16))
            .RuleFor(x => x.Width, f => f.Random.Int(320, 3840))
            .RuleFor(x => x.DisplayTitle, f => f.Lorem.Sentence(3))
            .RuleFor(x => x.ExtendedDisplayTitle, f => f.Lorem.Sentence(5))
            .RuleFor(x => x.Selected, f => f.Random.Bool())
            .RuleFor(x => x.Forced, f => f.Random.Bool())
            .RuleFor(x => x.Channels, f => f.Random.Int(1, 8))
            .RuleFor(x => x.AudioChannelLayout, f => f.PickRandom("5.1", "7.1", "2.0"))
            .RuleFor(x => x.SamplingRate, f => f.PickRandom(44100, 48000, 96000))
            .RuleFor(x => x.CanAutoSync, f => f.Random.Bool())
            .RuleFor(x => x.HearingImpaired, f => f.Random.Bool())
            .RuleFor(x => x.Dub, f => f.Random.Bool())
            .RuleFor(x => x.Title, f => f.Lorem.Word());
}
