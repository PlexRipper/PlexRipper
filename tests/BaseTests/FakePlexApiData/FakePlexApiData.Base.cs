using System.Net;
using System.Text.Json;

namespace Reaparr.BaseTests;

public static partial class FakePlexApiData
{
    private static int _uniqueNumber = 1;

    private static int GetUniqueNumber() => Interlocked.Increment(ref _uniqueNumber);

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
