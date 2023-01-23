using System.Text.Json;
using HttpClient.Contracts;
using PlexRipper.PlexApi.Converters;
using RestSharp;

namespace PlexRipper.HttpClient;

public class PlexRipperHttpClient : IPlexRipperHttpClient
{
    private readonly RestClient _client;

    public static JsonSerializerOptions SerializerOptions => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new LongToDateTime() },
    };

    public PlexRipperHttpClient(System.Net.Http.HttpClient httpClient)
    {
        var options = new RestClientOptions()
        {
            MaxTimeout = 10000,
            ThrowOnAnyError = false,
        };
        _client = new RestClient(httpClient, options);

        //_client.UseSystemTextJson(SerializerOptions);
        //_client.UseDotNetXmlSerializer();
    }

    public Task<Stream> DownloadStream(RestRequest request)
    {
        // TODO Add retry policy here
        return _client.DownloadStreamAsync(request);
    }
}