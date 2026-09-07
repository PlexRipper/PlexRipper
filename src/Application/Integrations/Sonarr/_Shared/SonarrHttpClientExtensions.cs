namespace Reaparr.Application;

public static class SonarrHttpClientExtensions
{
    public static async Task<Result> TestAllSonarrDownloadClientsAsync(this HttpClient client, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v3/downloadclient/testall");
        var result = await client.SendResultAsync(request, cancellationToken: ct);
        return result.Has400BadRequestError() ? Result.Ok() : result.ToResult();
    }
}
