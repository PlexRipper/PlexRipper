using System.Net;
using System.Text.Json;
using Bogus;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using PlexRipper.Domain.Config;

namespace PlexRipper.BaseTests;

public partial class FakePlexApiData
{
    public static Faker<GetServerResourcesResponse> GetServerResourcesResponse(
        HttpStatusCode statusCode,
        Action<PlexApiDataConfig>? options = null
    )
    {
        var config = PlexApiDataConfig.FromOptions(options);

        return new Faker<GetServerResourcesResponse>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.StatusCode, _ => (int)statusCode)
            .RuleFor(x => x.ContentType, _ => ContentType.ApplicationJson)
            .RuleFor(x => x.PlexDevices, _ => GetServerResource(options).Generate(config.PlexServerAccessCount))
            .RuleFor(x => x.RawResponse, (f, res) => GetHttpResponseMessage(statusCode, res.PlexDevices, null));
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
