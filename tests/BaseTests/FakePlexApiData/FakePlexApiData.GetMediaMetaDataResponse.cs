using System.Net;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.BaseTests;

public partial class FakePlexApiData
{
    public static GetMediaMetaDataResponse GetMediaMetaDataAsync(
        HttpStatusCode statusCode,
        Seed seed,
        GetAllLibrariesDirectory library,
        GetMediaMetaDataResponseBody? responseBody = null,
        HttpRequestMessage? request = null,
        Action<PlexApiDataConfig>? options = null
    )
    {
        return new Faker<GetMediaMetaDataResponse>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.StatusCode, _ => (int)statusCode)
            .RuleFor(x => x.ContentType, _ => ContentType.ApplicationJson)
            .RuleFor(
                x => x.Object,
                _ => responseBody ?? GetMediaMetaDataResponseBodyResponse(seed, library, 100, options)
            )
            .RuleFor(x => x.RawResponse, (_, res) => GetHttpResponseMessage(statusCode, res.Object, request))
            .Generate();
    }

    public static GetMediaMetaDataResponseBody GetMediaMetaDataResponseBodyResponse(
        Seed seed,
        GetAllLibrariesDirectory library,
        int mediaCount = 0,
        Action<PlexApiDataConfig>? options = null
    )
    {
        var type = library.Type.ToPlexMediaType();

        return new GetMediaMetaDataResponseBody
        {
            MediaContainer = _getMediaMetaDataMediaContainer
                .UseSeed(seed.Next())
                .FinishWith(
                    (_, x) =>
                    {
                        x.LibrarySectionID = long.Parse(library.Key);
                        x.LibrarySectionTitle = library.Title;
                        x.LibrarySectionUUID = library.Uuid;
                        x.Metadata = GetMediaMetaDataMetadata(seed, type, options).Generate(mediaCount);
                        x.Size = x.Metadata!.Count;
                    }
                )
                .Generate(),
        };
    }

    public static Faker<GetMediaMetaDataMetadata> GetMediaMetaDataMetadata(
        Seed seed,
        PlexMediaType type,
        Action<PlexApiDataConfig>? options = null
    )
    {
        var config = PlexApiDataConfig.FromOptions(options);
        return _getMediaMetaDataMetadata
            .UseSeed(seed.Next())
            .RuleFor(l => l.Role, _ => _getMediaMetaDataRole.GenerateUnique(config.RolePerMediaItemCount, x => x.Tag))
            .RuleFor(
                l => l.Genre,
                _ => _getMediaMetaDataGenre.GenerateUnique(config.GenrePerMediaItemCount, x => x.Tag)
            )
            .RuleFor(
                l => l.Country,
                _ => _getMediaMetaDataCountry.GenerateUnique(config.CountriesPerMediaItemCount, x => x.Tag)
            )
            .FinishWith(
                (f, x) =>
                {
                    x.Type = type.ToGetMediaMetaDataType();
                    x.Media = [GetMediaMetaDataMedia(seed, options).Generate()];
                    x.Guid = f.PlexMedia().Guid(type);
                }
            );
    }

    public static Faker<GetMediaMetaDataMedia> GetMediaMetaDataMedia(
        Seed seed,
        Action<PlexApiDataConfig>? options = null
    )
    {
        return _getMediaMetaDataMedia
            .UseSeed(seed.Next())
            .FinishWith(
                (_, x) =>
                {
                    x.Part = [GetMediaMetaDataPart(seed, options).Generate()];
                }
            );
    }

    public static Faker<GetMediaMetaDataPart> GetMediaMetaDataPart(
        Seed seed,
        Action<PlexApiDataConfig>? options = null
    ) =>
        _getMediaMetaDataPartFaker
            .UseSeed(seed.Next())
            .FinishWith(
                (_, x) =>
                {
                    x.Stream = [_getMediaMetaDataStreamFaker.Generate()];
                }
            );
}
