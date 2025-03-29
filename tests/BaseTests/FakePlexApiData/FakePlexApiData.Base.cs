using System.Net;
using System.Text.Json;

namespace PlexRipper.BaseTests;

public static partial class FakePlexApiData
{
    private static readonly Random RandomInstance = new();

    private static readonly HashSet<int> AlreadyGenerated = [0];

    private static int GetUniqueNumber()
    {
        if (AlreadyGenerated.Count >= int.MaxValue)
            throw new InvalidOperationException("All possible unique numbers have been generated.");

        var value = 0;
        while (AlreadyGenerated.Contains(value))
        {
            value = RandomInstance.Next(1, int.MaxValue);
        }

        AlreadyGenerated.Add(value);
        return value;
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
