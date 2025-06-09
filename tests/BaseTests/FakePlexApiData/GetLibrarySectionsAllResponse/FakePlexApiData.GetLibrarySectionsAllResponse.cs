using System.Net;
using Bogus.Hollywood;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using NodaTime;
using PlexApi.Contracts;
using PlexRipper.PlexApi;

namespace PlexRipper.BaseTests;

public partial class FakePlexApiData
{
    public static GetLibrarySectionsAllResponse GetLibrarySectionsAllResponse(
        HttpStatusCode statusCode,
        Seed seed,
        GetAllLibrariesDirectory library,
        GetLibrarySectionsAllResponseBody? responseBody = null,
        HttpRequestMessage? request = null,
        Action<PlexApiDataConfig>? options = null
    )
    {
        return new Faker<GetLibrarySectionsAllResponse>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.StatusCode, _ => (int)statusCode)
            .RuleFor(x => x.ContentType, _ => ContentType.ApplicationJson)
            .RuleFor(x => x.Object, _ => responseBody ?? GetLibrarySectionsAllResponseBody(seed, library, 100, options))
            .RuleFor(x => x.RawResponse, (_, res) => GetHttpResponseMessage(statusCode, res.Object, request))
            .Generate();
    }

    public static GetLibrarySectionsAllResponseBody GetLibrarySectionsAllResponseBody(
        Seed seed,
        GetAllLibrariesDirectory library,
        int mediaCount = 0,
        Action<PlexApiDataConfig>? options = null
    )
    {
        var config = PlexApiDataConfig.FromOptions(options);
        var type = library.Type.ToPlexMediaType();

        return new GetLibrarySectionsAllResponseBody
        {
            MediaContainer = _getLibrarySectionsAllMediaContainer
                .UseSeed(seed.Next())
                .FinishWith(
                    (_, x) =>
                    {
                        x.LibrarySectionID = long.Parse(library.Key);
                        x.LibrarySectionTitle = library.Title;
                        x.LibrarySectionUUID = library.Uuid;
                        x.Metadata = GetLibrarySectionsAllMetadata(seed, type, options).Generate(mediaCount);
                        x.Size = x.Metadata!.Count;
                    }
                )
                .Generate(),
        };
    }

    public static Faker<GetLibrarySectionsAllMetadata> GetLibrarySectionsAllMetadata(
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
                    x.Type = type.ToGetLibrarySectionsAllLibraryType();
                    x.Media = [GetLibrarySectionsAllMedia(seed, options).Generate()];
                    x.Guid = f.PlexMedia().Guid(type);
                }
            );
    }

    public static Faker<GetLibrarySectionsAllMedia> GetLibrarySectionsAllMedia(
        Seed seed,
        Action<PlexApiDataConfig>? options = null
    )
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

    public static Faker<GetLibrarySectionsAllPart> GetLibrarySectionsAllPart(
        Seed seed,
        Action<PlexApiDataConfig>? options = null
    ) =>
        _getLibrarySectionsAllPartFaker
            .UseSeed(seed.Next())
            .FinishWith(
                (_, x) =>
                {
                    x.Stream = [_getLibrarySectionsAllStreamFaker.Generate()];
                }
            );
}
