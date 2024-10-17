using System.Net;
using System.Text.Json;
using Bogus;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using PlexRipper.Domain.Config;

namespace PlexRipper.BaseTests;

public partial class FakePlexApiData
{
    public static GetServerResourcesResponse GetServerResourcesResponse(
        HttpStatusCode statusCode,
        Seed seed,
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
            .RuleFor(x => x.RawResponse, (_, res) => GetHttpResponseMessage(statusCode, res.PlexDevices, null))
            .Generate();
    }

    public static GetAllLibrariesResponse GetAllLibrariesResponse(
        HttpStatusCode statusCode,
        Seed seed,
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
            .RuleFor(x => x.RawResponse, (_, res) => GetHttpResponseMessage(statusCode, res.Object, null))
            .Generate();
    }

    public static GetServerIdentityResponseBody GetPlexServerIdentityResponse(
        Seed seed,
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

        return new Faker<GetServerIdentityResponseBody>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.MediaContainer, _ => container.Generate())
            .Generate();
    }

    public static HttpResponseMessage GetHttpResponseMessage<T>(
        HttpStatusCode statusCode,
        T data,
        HttpRequestMessage? request
    )
        where T : class?
    {
        var json = JsonSerializer.Serialize(data, DefaultJsonSerializerOptions.ConfigStandard);

        // Fix casing of protocol to match the actual API response, which is lowercase
        // Fixes retarted enum conversion in PlexApi.SDK
        json = json.Replace("\"protocol\":\"Http\"", "\"protocol\":\"http\"");
        json = json.Replace("\"protocol\":\"Https\"", "\"protocol\":\"https\"");

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
