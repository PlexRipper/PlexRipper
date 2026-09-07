using System.Net.Http.Headers;

namespace Reaparr.Application;

public interface IRadarrHttpClientFactory
{
    Task<Result<HttpClient>> CreateAsync(Guid integrationId);

    Result<HttpClient> Create(string baseUrl, string apiKey);
}

public class RadarrHttpClientFactory : IRadarrHttpClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IReaparrDbContextFactory _dbContextFactory;

    public RadarrHttpClientFactory(IHttpClientFactory httpClientFactory, IReaparrDbContextFactory dbContextFactory)
    {
        _httpClientFactory = httpClientFactory;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Result<HttpClient>> CreateAsync(Guid integrationId)
    {
        var result = await Result.Try(async Task<(string BaseUrl, string ApiKey)?> () =>
        {
            using var dbContext = await _dbContextFactory.CreateAsync();
            var integration = await dbContext
                .RadarrIntegrations.Where(x => x.Id == integrationId)
                .Select(x => new { x.BaseUrl, ApiKey = x.RadarrApiKey })
                .SingleOrDefaultAsync(CancellationToken.None);
            return integration is null ? null : (integration.BaseUrl, integration.ApiKey);
        });
        if (result.IsFailed)
            return result.ToResult().LogError();
        if (result.Value is null)
            return ResultExtensions.EntityNotFound(nameof(RadarrIntegration), integrationId);

        return Create(result.Value.Value.BaseUrl, result.Value.Value.ApiKey);
    }

    public Result<HttpClient> Create(string baseUrl, string apiKey) =>
        Result.Try(() =>
        {
            var client = _httpClientFactory.CreateClient(HttpClientModule.DefaultClientName);
            client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
            return client;
        });
}
