using System.Net;

namespace Reaparr.Application;

public static class RadarrHttpClientExtensions
{
    private static readonly ILogger _log = LogFactory.Create(typeof(RadarrHttpClientExtensions));

    public static async Task<Result<TestConnectionResult>> TestRadarrConnectionAsync(
        this HttpClient client,
        CancellationToken ct
    )
    {
        var result = await client.SendRadarrAsync(new HttpRequestMessage(HttpMethod.Get, "api/v3/system/status"), ct);
        if (result.IsCancelled)
            return result.ToResult<TestConnectionResult>().LogWarning();
        if (result.HasException<TaskCanceledException>())
        {
            _log.Here().Warning("Radarr connection test timed out");
            return Result.Ok(
                CreateConnectionResult(TestConnectionStatus.ConnectionFailed, null, "Connection timed out.")
            );
        }
        if (result.HasException<HttpRequestException>())
        {
            _log.Here().Warning("Radarr connection test failed");
            return Result.Ok(CreateConnectionResult(TestConnectionStatus.ConnectionFailed, null, "Connection failed."));
        }
        if (result.IsFailed)
            return result.ToResult<TestConnectionResult>().LogError();

        var response = result.Value;
        var statusCode = (int)response.StatusCode;
        if (response.IsSuccessStatusCode)
            return Result.Ok(CreateConnectionResult(TestConnectionStatus.Success, statusCode, null));

        var status =
            response.StatusCode == HttpStatusCode.Unauthorized
                ? TestConnectionStatus.InvalidApiKey
                : TestConnectionStatus.ConnectionFailed;
        var error = response.ReasonPhrase ?? $"HTTP {statusCode}";
        _log.Here().Warning("Radarr connection test failed: {Reason}", error);
        return Result.Ok(CreateConnectionResult(status, statusCode, error));
    }

    public static Task<Result<List<RadarrDownloadClientResourceDTO>>> GetRadarrDownloadClientsAsync(
        this HttpClient client,
        CancellationToken ct
    ) =>
        client.SendRadarrAsync(
            new HttpRequestMessage(HttpMethod.Get, "api/v3/downloadclient"),
            "Failed to get download clients from Radarr",
            static body =>
                JsonSerializer.Deserialize<List<RadarrDownloadClientResourceDTO>>(
                    body,
                    DefaultJsonSerializerOptions.ConfigStandard
                ) ?? [],
            ct
        );

    public static Task<Result<RadarrDownloadClientResourceDTO>> CreateRadarrDownloadClientAsync(
        this HttpClient client,
        bool forceSave,
        RadarrDownloadContractDTO resource,
        CancellationToken ct
    )
    {
        _log.Here().Debug("Creating Radarr download client with name {DownloadClientName}", resource.Name);
        return client.SendRadarrAsync(
            CreateJsonRequest(
                HttpMethod.Post,
                $"api/v3/downloadclient?forceSave={forceSave.ToString().ToLowerInvariant()}",
                resource
            ),
            "Failed to create download client in Radarr",
            static body =>
                JsonSerializer.Deserialize<RadarrDownloadClientResourceDTO>(
                    body,
                    DefaultJsonSerializerOptions.ConfigStandard
                ) ?? new RadarrDownloadClientResourceDTO(),
            ct
        );
    }

    public static Task<Result<RadarrDownloadClientResourceDTO>> UpdateRadarrDownloadClientAsync(
        this HttpClient client,
        int id,
        bool forceSave,
        RadarrDownloadContractDTO resource,
        CancellationToken ct
    )
    {
        _log.Here().Debug("Updating Radarr download client with name {DownloadClientName}", resource.Name);
        return client.SendRadarrAsync(
            CreateJsonRequest(
                HttpMethod.Put,
                $"api/v3/downloadclient/{id}".SetQueryParam("forceSave", forceSave).ToString(),
                resource
            ),
            "Failed to update download client in Radarr",
            static body =>
                JsonSerializer.Deserialize<RadarrDownloadClientResourceDTO>(
                    body,
                    DefaultJsonSerializerOptions.ConfigStandard
                ) ?? new RadarrDownloadClientResourceDTO(),
            ct
        );
    }

    public static Task<Result<List<RadarrIndexerResourceDTO>>> GetRadarrIndexersAsync(
        this HttpClient client,
        CancellationToken ct
    ) =>
        client.SendRadarrAsync(
            new HttpRequestMessage(HttpMethod.Get, "api/v3/indexer"),
            "Failed to get indexers from Radarr",
            static body =>
                JsonSerializer.Deserialize<List<RadarrIndexerResourceDTO>>(
                    body,
                    DefaultJsonSerializerOptions.ConfigStandard
                ) ?? [],
            ct
        );

    public static Task<Result<RadarrIndexerResourceDTO>> CreateRadarrIndexerAsync(
        this HttpClient client,
        bool forceSave,
        RadarrIndexerContractDTO resource,
        CancellationToken ct
    )
    {
        _log.Here().Debug("Creating Radarr indexer with name {IndexerName}", resource.Name);
        return client.SendRadarrAsync(
            CreateJsonRequest(
                HttpMethod.Post,
                $"api/v3/indexer?forceSave={forceSave.ToString().ToLowerInvariant()}",
                resource
            ),
            "Failed to create indexer in Radarr",
            static body =>
                JsonSerializer.Deserialize<RadarrIndexerResourceDTO>(body, DefaultJsonSerializerOptions.ConfigStandard)
                ?? new RadarrIndexerResourceDTO(),
            ct
        );
    }

    public static Task<Result<RadarrIndexerResourceDTO>> UpdateRadarrIndexerAsync(
        this HttpClient client,
        int id,
        bool forceSave,
        RadarrIndexerContractDTO resource,
        CancellationToken ct
    )
    {
        _log.Here().Debug("Updating Radarr indexer with name {IndexerName}", resource.Name);
        return client.SendRadarrAsync(
            CreateJsonRequest(
                HttpMethod.Put,
                $"api/v3/indexer/{id}?forceSave={forceSave.ToString().ToLowerInvariant()}",
                resource
            ),
            "Failed to update indexer in Radarr",
            static body =>
                JsonSerializer.Deserialize<RadarrIndexerResourceDTO>(body, DefaultJsonSerializerOptions.ConfigStandard)
                ?? new RadarrIndexerResourceDTO(),
            ct
        );
    }

    public static Task<Result> TestRadarrDownloadClientAsync(
        this HttpClient client,
        RadarrDownloadContractDTO resource,
        CancellationToken ct
    ) => client.TestRadarrResourceAsync("api/v3/downloadclient/test", resource, "download client", ct);

    public static Task<Result> TestRadarrIndexerAsync(
        this HttpClient client,
        RadarrIndexerContractDTO resource,
        CancellationToken ct
    ) => client.TestRadarrResourceAsync("api/v3/indexer/test", resource, "indexer", ct);

    public static async Task<Result> DeleteRadarrResourcesAsync(
        this HttpClient client,
        int? externalIndexerId,
        int? externalDownloadClientId,
        CancellationToken ct
    )
    {
        foreach (
            var resource in new[]
            {
                (Name: "indexer", Id: externalIndexerId),
                (Name: "downloadclient", Id: externalDownloadClientId),
            }
        )
        {
            if (resource.Id is null)
                continue;

            var result = await client.SendRadarrAsync(
                new HttpRequestMessage(HttpMethod.Delete, $"api/v3/{resource.Name}/{resource.Id.Value}"),
                ct
            );
            if (result.IsFailed)
                return result.ToResult().LogIfFailed();
            if (!result.Value.IsSuccessStatusCode && result.Value.StatusCode != HttpStatusCode.NotFound)
                return Result.Fail(
                    $"Failed to delete Radarr {resource.Name} {resource.Id.Value}: {result.Value.StatusCode}."
                );
        }

        return Result.Ok();
    }

    public static async Task<Result> TestAllRadarrDownloadClientsAsync(this HttpClient client, CancellationToken ct)
    {
        var result = await client.SendRadarrAsync(
            new HttpRequestMessage(HttpMethod.Post, "api/v3/downloadclient/testall"),
            ct
        );
        if (result.IsFailed)
            return result.ToResult().LogIfFailed();
        return result.Value.IsSuccessStatusCode || result.Value.StatusCode == HttpStatusCode.BadRequest
            ? Result.Ok()
            : Result.Fail($"Failed to test Radarr download clients. StatusCode: {result.Value.StatusCode}");
    }

    private static async Task<Result> TestRadarrResourceAsync<T>(
        this HttpClient client,
        string path,
        T resource,
        string resourceName,
        CancellationToken ct
    )
    {
        var result = await client.SendRadarrAsync(CreateJsonRequest(HttpMethod.Post, path, resource), ct);
        if (result.IsFailed)
            return result.ToResult().LogIfFailed();
        return result.Value.IsSuccessStatusCode
            ? Result.Ok()
            : Result
                .Fail($"Failed to validate Reaparr {resourceName} in Radarr. StatusCode: {result.Value.StatusCode}")
                .WithError(result.Value.Body)
                .LogError();
    }

    private static async Task<Result<T>> SendRadarrAsync<T>(
        this HttpClient client,
        HttpRequestMessage request,
        string failureMessage,
        Func<string, T> deserialize,
        CancellationToken ct
    )
    {
        var result = await client.SendRadarrAsync(request, ct);
        if (result.IsFailed)
            return result.ToResult<T>().LogIfFailed();
        if (!result.Value.IsSuccessStatusCode)
            return Result
                .Fail($"{failureMessage}. StatusCode: {result.Value.StatusCode}")
                .WithError(result.Value.Body)
                .LogError();

        return Result.Ok(deserialize(result.Value.Body));
    }

    private static async Task<Result<RadarrHttpResponse>> SendRadarrAsync(
        this HttpClient client,
        HttpRequestMessage request,
        CancellationToken ct
    ) =>
        await Result.Try(async Task<RadarrHttpResponse> () =>
        {
            using (request)
            using (var response = await client.SendAsync(request, ct))
            {
                return new RadarrHttpResponse(
                    response.StatusCode,
                    response.ReasonPhrase,
                    await response.Content.ReadAsStringAsync(ct)
                );
            }
        });

    private static HttpRequestMessage CreateJsonRequest<T>(HttpMethod method, string path, T value)
    {
        var request = new HttpRequestMessage(method, path);
        request.Content = JsonSerializer
            .Serialize(value, DefaultJsonSerializerOptions.ConfigStandard)
            .ToStringContent();
        return request;
    }

    private static TestConnectionResult CreateConnectionResult(
        TestConnectionStatus status,
        int? httpStatusCode,
        string? errorMessage
    ) =>
        new()
        {
            Status = status,
            HttpStatusCode = httpStatusCode,
            ErrorMessage = errorMessage,
            TestedAt = DateTime.UtcNow,
        };

    private sealed record RadarrHttpResponse(HttpStatusCode StatusCode, string? ReasonPhrase, string Body)
    {
        public bool IsSuccessStatusCode => (int)StatusCode is >= 200 and <= 299;
    }
}
