using System.Net;
using LukeHagar.PlexAPI.SDK.Models.Components;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using Reaparr.PlexApi;
using Metadata = LukeHagar.PlexAPI.SDK.Models.Components.Metadata;

namespace Reaparr.BaseTests;

public partial class FakePlexApiData
{
    public static GetMetadataItemResponse GetLibrarySectionsAllResponse(
        HttpStatusCode statusCode,
        Seed seed,
        LibrarySection library,
        MediaContainerWithMetadata? responseBody = null,
        HttpRequestMessage? request = null,
        Action<PlexApiDataConfig>? options = null
    )
    {
        return new Faker<GetMetadataItemResponse>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.StatusCode, _ => (int)statusCode)
            .RuleFor(x => x.ContentType, _ => ContentType.ApplicationJson)
            .Ignore(x => x.Headers)
            .RuleFor(
                x => x.MediaContainerWithMetadata,
                _ => responseBody ?? GetLibrarySectionsAllResponseBody(seed, library, 100, options)
            )
            .RuleFor(
                x => x.RawResponse,
                (_, res) => GetHttpResponseMessage(statusCode, res.MediaContainerWithMetadata, request)
            )
            .Generate();
    }

    public static MediaContainerWithMetadata GetLibrarySectionsAllResponseBody(
        Seed seed,
        LibrarySection library,
        int mediaCount = 0,
        Action<PlexApiDataConfig>? options = null
    )
    {
        var type = library.Type.ToPlexMediaType();

        return new MediaContainerWithMetadata
        {
            MediaContainer = _getLibrarySectionsAllMediaContainer
                .UseSeed(seed.Next())
                .FinishWith(
                    (_, x) =>
                    {
                        x.Metadata = GetLibrarySectionsAllMetadata(seed, type, options).Generate(mediaCount);
                        x.Size = x.Metadata!.Count;
                        x.TotalSize = x.Metadata!.Count;
                    }
                )
                .Generate(),
        };
    }

    public static Faker<Metadata> GetLibrarySectionsAllMetadata(
        Seed seed,
        PlexMediaType type,
        Action<PlexApiDataConfig>? options = null
    )
    {
        return _getLibrarySectionsAllMetadata
            .UseSeed(seed.Next())
            .FinishWith(
                (f, x) =>
                {
                    x.Type = type.ToPlexApiString();
                    x.Media = [GetLibrarySectionsAllMedia(seed, options).Generate()];
                    x.Guid = f.PlexMedia().Guid(type);
                }
            );
    }

    public static Faker<Media> GetLibrarySectionsAllMedia(Seed seed, Action<PlexApiDataConfig>? options = null)
    {
        return _getLibrarySectionsAllMedia
            .UseSeed(seed.Next())
            .FinishWith(
                (_, x) =>
                {
                    x.Part = [GetLibrarySectionsAllPart(seed, options).Generate()];
                }
            );
    }

    public static Faker<Part> GetLibrarySectionsAllPart(Seed seed, Action<PlexApiDataConfig>? options = null) =>
        _getLibrarySectionsAllPartFaker
            .UseSeed(seed.Next())
            .FinishWith(
                (_, x) =>
                {
                    x.Stream = [_getLibrarySectionsAllStreamFaker.Generate()];
                }
            );
}
