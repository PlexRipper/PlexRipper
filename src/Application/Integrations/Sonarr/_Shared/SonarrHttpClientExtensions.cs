using System.Net;

namespace Reaparr.Application;

public static class SonarrHttpClientExtensions
{
    private static readonly ILogger _log = LogFactory.Create(typeof(SonarrHttpClientExtensions));

    public static async Task<Result<TestConnectionResult>> TestSonarrConnectionAsync(
        this HttpClient client,
        CancellationToken ct
    )
    {
        var result = await client.SendSonarrAsync(new HttpRequestMessage(HttpMethod.Get, "api/v3/system/status"), ct);
        if (result.IsCancelled)
            return result.ToResult<TestConnectionResult>().LogWarning();
        if (result.HasException<TaskCanceledException>())
        {
            _log.Here().Warning("Sonarr connection test timed out");
            return Result.Ok(
                CreateConnectionResult(TestConnectionStatus.ConnectionFailed, null, "Connection timed out.")
            );
        }
        if (result.HasException<HttpRequestException>())
        {
            _log.Here().Warning("Sonarr connection test failed");
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
        _log.Here().Warning("Sonarr connection test failed: {Reason}", error);
        return Result.Ok(CreateConnectionResult(status, statusCode, error));
    }

    public static Task<Result<List<DownloadClientResourceDTO>>> GetSonarrDownloadClientsAsync(
        this HttpClient client,
        CancellationToken ct
    ) =>
        client.SendSonarrAsync(
            new HttpRequestMessage(HttpMethod.Get, "api/v3/downloadclient"),
            "Failed to get download clients from Sonarr",
            static body =>
                JsonSerializer.Deserialize<List<DownloadClientResourceDTO>>(
                    body,
                    DefaultJsonSerializerOptions.ConfigStandard
                ) ?? [],
            ct
        );

    public static Task<Result<SonarrDownloadContractDTO>> CreateSonarrDownloadClientAsync(
        this HttpClient client,
        bool forceSave,
        SonarrDownloadContractDTO resource,
        CancellationToken ct
    )
    {
        _log.Here().Debug("Creating Sonarr download client with name {DownloadClientName}", resource.Name);
        return client.SendSonarrAsync(
            CreateJsonRequest(
                HttpMethod.Post,
                $"api/v3/downloadclient?forceSave={forceSave.ToString().ToLowerInvariant()}",
                resource
            ),
            "Failed to create download client in Sonarr",
            static body =>
                JsonSerializer.Deserialize<SonarrDownloadContractDTO>(body, DefaultJsonSerializerOptions.ConfigStandard)
                ?? new SonarrDownloadContractDTO(),
            ct
        );
    }

    public static Task<Result<SonarrDownloadContractDTO>> UpdateSonarrDownloadClientAsync(
        this HttpClient client,
        int id,
        bool forceSave,
        SonarrDownloadContractDTO resource,
        CancellationToken ct
    )
    {
        _log.Here().Debug("Updating Sonarr download client with name {DownloadClientName}", resource.Name);
        return client.SendSonarrAsync(
            CreateJsonRequest(
                HttpMethod.Put,
                $"api/v3/downloadclient/{id}?forceSave={forceSave.ToString().ToLowerInvariant()}",
                resource
            ),
            "Failed to update download client in Sonarr",
            static body =>
                JsonSerializer.Deserialize<SonarrDownloadContractDTO>(body, DefaultJsonSerializerOptions.ConfigStandard)
                ?? new SonarrDownloadContractDTO(),
            ct
        );
    }

    public static Task<Result<List<IndexerResourceDTO>>> GetSonarrIndexersAsync(
        this HttpClient client,
        CancellationToken ct
    ) =>
        client.SendSonarrAsync(
            new HttpRequestMessage(HttpMethod.Get, "api/v3/indexer"),
            "Failed to get indexers from Sonarr",
            static body =>
                JsonSerializer.Deserialize<List<IndexerResourceDTO>>(body, DefaultJsonSerializerOptions.ConfigStandard)
                ?? [],
            ct
        );

    public static Task<Result<SonarrIndexerContractDTO>> CreateSonarrIndexerAsync(
        this HttpClient client,
        bool forceSave,
        SonarrIndexerContractDTO resource,
        CancellationToken ct
    )
    {
        _log.Here().Debug("Creating Sonarr indexer with name {IndexerName}", resource.Name);
        return client.SendSonarrAsync(
            CreateJsonRequest(
                HttpMethod.Post,
                $"api/v3/indexer?forceSave={forceSave.ToString().ToLowerInvariant()}",
                resource
            ),
            "Failed to create indexer in Sonarr",
            static body =>
                JsonSerializer.Deserialize<SonarrIndexerContractDTO>(body, DefaultJsonSerializerOptions.ConfigStandard)
                ?? new SonarrIndexerContractDTO(),
            ct
        );
    }

    public static Task<Result<SonarrIndexerContractDTO>> UpdateSonarrIndexerAsync(
        this HttpClient client,
        int id,
        bool forceSave,
        SonarrIndexerContractDTO resource,
        CancellationToken ct
    )
    {
        _log.Here().Debug("Updating Sonarr indexer with name {IndexerName}", resource.Name);
        return client.SendSonarrAsync(
            CreateJsonRequest(
                HttpMethod.Put,
                $"api/v3/indexer/{id}?forceSave={forceSave.ToString().ToLowerInvariant()}",
                resource
            ),
            "Failed to update indexer in Sonarr",
            static body =>
                JsonSerializer.Deserialize<SonarrIndexerContractDTO>(body, DefaultJsonSerializerOptions.ConfigStandard)
                ?? new SonarrIndexerContractDTO(),
            ct
        );
    }

    public static Task<Result> TestSonarrDownloadClientAsync(
        this HttpClient client,
        SonarrDownloadContractDTO resource,
        CancellationToken ct
    ) => client.TestSonarrResourceAsync("api/v3/downloadclient/test", resource, "download client", ct);

    public static Task<Result> TestSonarrIndexerAsync(
        this HttpClient client,
        SonarrIndexerContractDTO resource,
        CancellationToken ct
    ) => client.TestSonarrResourceAsync("api/v3/indexer/test", resource, "indexer", ct);

    public static async Task<Result> DeleteSonarrResourcesAsync(
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

            var result = await client.SendSonarrAsync(
                new HttpRequestMessage(HttpMethod.Delete, $"api/v3/{resource.Name}/{resource.Id.Value}"),
                ct
            );
            if (result.IsCancelled)
                return result.ToResult().LogWarning();
            if (result.IsFailed)
                return result.ToResult().LogError();
            if (!result.Value.IsSuccessStatusCode && result.Value.StatusCode != HttpStatusCode.NotFound)
                return Result.Fail(
                    $"Failed to delete Sonarr {resource.Name} {resource.Id.Value}: {result.Value.StatusCode}."
                );
        }

        return Result.Ok();
    }

    public static async Task<Result> TestAllSonarrDownloadClientsAsync(this HttpClient client, CancellationToken ct)
    {
        var result = await client.SendSonarrAsync(
            new HttpRequestMessage(HttpMethod.Post, "api/v3/downloadclient/testall"),
            ct
        );
        if (result.IsCancelled)
            return result.ToResult().LogWarning();
        if (result.IsFailed)
            return result.ToResult().LogError();
        return result.Value.IsSuccessStatusCode || result.Value.StatusCode == HttpStatusCode.BadRequest
            ? Result.Ok()
            : Result.Fail($"Failed to test Sonarr download clients. StatusCode: {result.Value.StatusCode}");
    }

    private static async Task<Result> TestSonarrResourceAsync<T>(
        this HttpClient client,
        string path,
        T resource,
        string resourceName,
        CancellationToken ct
    )
    {
        var result = await client.SendSonarrAsync(CreateJsonRequest(HttpMethod.Post, path, resource), ct);
        if (result.IsCancelled)
            return result.ToResult().LogWarning();
        if (result.IsFailed)
            return result.ToResult().LogError();
        return result.Value.IsSuccessStatusCode
            ? Result.Ok()
            : Result
                .Fail($"Failed to validate Reaparr {resourceName} in Sonarr. StatusCode: {result.Value.StatusCode}")
                .WithError(result.Value.Body)
                .LogError();
    }

    private static async Task<Result<T>> SendSonarrAsync<T>(
        this HttpClient client,
        HttpRequestMessage request,
        string failureMessage,
        Func<string, T> deserialize,
        CancellationToken ct
    )
    {
        var result = await client.SendSonarrAsync(request, ct);
        if (result.IsCancelled)
            return result.ToResult<T>().LogWarning();
        if (result.IsFailed)
            return result.ToResult<T>().LogError();
        if (!result.Value.IsSuccessStatusCode)
            return Result
                .Fail($"{failureMessage}. StatusCode: {result.Value.StatusCode}")
                .WithError(result.Value.Body)
                .LogError();

        return Result.Ok(deserialize(result.Value.Body));
    }

    private static async Task<Result<SonarrHttpResponse>> SendSonarrAsync(
        this HttpClient client,
        HttpRequestMessage request,
        CancellationToken ct
    ) =>
        await Result.Try(async Task<SonarrHttpResponse> () =>
        {
            using (request)
            using (var response = await client.SendAsync(request, ct))
            {
                return new SonarrHttpResponse(
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

    private sealed record SonarrHttpResponse(HttpStatusCode StatusCode, string? ReasonPhrase, string Body)
    {
        public bool IsSuccessStatusCode => (int)StatusCode is >= 200 and <= 299;
    }
}
