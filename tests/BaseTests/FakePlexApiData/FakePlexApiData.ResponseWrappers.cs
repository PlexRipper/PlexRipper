using System.Net;
using System.Text.Json;
using Bogus;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using PlexRipper.Domain.Config;

namespace PlexRipper.BaseTests;

public partial class FakePlexApiData
{
    /// <summary>
    /// Allows you to generate a fake response for the PostUsersSignInData operation
    /// Works with HttpStatusCode.Created and HttpStatusCode.Unauthorized
    /// </summary>
    /// <param name="statusCode"> The status code to return </param>
    /// <param name="seed"> The seed to use for the faker data </param>
    /// <param name="request"> The request message that was sent </param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static PostUsersSignInDataResponse PostUsersSignInDataResponse(
        HttpStatusCode statusCode,
        Seed seed,
        HttpRequestMessage? request = null,
        Action<PlexApiDataConfig>? options = null
    )
    {
        return new Faker<PostUsersSignInDataResponse>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.StatusCode, _ => (int)statusCode)
            .RuleFor(x => x.ContentType, _ => ContentType.ApplicationJson)
            .RuleFor(
                x => x.UserPlexAccount,
                _ => statusCode == HttpStatusCode.Created ? GetPlexSignInResponse(seed, options).Generate() : null
            )
            .RuleFor(
                x => x.RawResponse,
                (_, res) =>
                    statusCode == HttpStatusCode.Created
                        ? GetHttpResponseMessage(statusCode, res.UserPlexAccount, request)
                        : GetHttpResponseMessage(statusCode, GetFailedPlexSignInResponse(), request)
            )
            .Generate();
    }

    public static GetServerResourcesResponse GetServerResourcesResponse(
        HttpStatusCode statusCode,
        Seed seed,
        HttpRequestMessage? request = null,
        Action<PlexApiDataConfig>? options = null
    )
    {
        var config = PlexApiDataConfig.FromOptions(options);

        return new Faker<GetServerResourcesResponse>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.StatusCode, _ => (int)statusCode)
            .RuleFor(x => x.ContentType, _ => ContentType.ApplicationJson)
            .RuleFor(x => x.PlexDevices, _ => GetServerResource(seed, options).Generate(config.PlexServerAccessCount))
            .RuleFor(x => x.RawResponse, (_, res) => GetHttpResponseMessage(statusCode, res.PlexDevices, request))
            .Generate();
    }

    public static GetAllLibrariesResponse GetAllLibrariesResponse(
        HttpStatusCode statusCode,
        Seed seed,
        HttpRequestMessage? request = null,
        Action<PlexApiDataConfig>? options = null
    )
    {
        var config = PlexApiDataConfig.FromOptions(options);

        return new Faker<GetAllLibrariesResponse>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.StatusCode, _ => (int)statusCode)
            .RuleFor(x => x.ContentType, _ => ContentType.ApplicationJson)
            .RuleFor(x => x.Object, _ => GetAllLibrariesResponseBody(seed, options))
            .RuleFor(x => x.RawResponse, (_, res) => GetHttpResponseMessage(statusCode, res.Object, request))
            .Generate();
    }

    public static GetServerIdentityResponse GetPlexServerIdentityResponse(
        HttpStatusCode statusCode,
        Seed seed,
        HttpRequestMessage? request = null,
        Action<PlexApiDataConfig>? options = null
    )
    {
        var container = new Faker<GetServerIdentityMediaContainer>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.Size, _ => 0)
            .RuleFor(x => x.Claimed, f => f.Random.Bool())
            .RuleFor(x => x.MachineIdentifier, f => f.PlexApi().MachineIdentifier)
            .RuleFor(x => x.Version, f => f.PlexApi().PlexVersion);

        var body = new Faker<GetServerIdentityResponseBody>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.MediaContainer, _ => container.Generate())
            .Generate();

        return new Faker<GetServerIdentityResponse>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.StatusCode, _ => (int)statusCode)
            .RuleFor(x => x.ContentType, _ => ContentType.ApplicationJson)
            .RuleFor(x => x.Object, _ => body)
            .RuleFor(x => x.RawResponse, (_, res) => GetHttpResponseMessage(statusCode, res.Object, request))
            .Generate();
    }

    public static HttpResponseMessage GetHttpResponseMessage<T>(
        HttpStatusCode statusCode,
        T data,
        HttpRequestMessage? request
    )
        where T : class?
    {
        var json = JsonSerializer.Serialize(data, DefaultJsonSerializerOptions.PlexApiSerialization);

        return new HttpResponseMessage
        {
            Content = json.ToStringContent(),
            ReasonPhrase = statusCode.ToString(),
            RequestMessage = request,
            StatusCode = statusCode,
            Version = new Version(1, 1),
        };
    }
}
