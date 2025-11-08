using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application;

public static class HttpClientModule
{
    internal static readonly string SonarrClientName = "Sonarr";

    public static void RegisterSonarrHttpClient(this IServiceCollection services)
    {
        services
            .AddHttpClient(
                SonarrClientName,
                (sp, client) =>
                {
                    var settings = sp.GetRequiredService<ISonarrSettings>();
                    if (settings.BaseUrl == string.Empty || settings.ApiKey == string.Empty)
                    {
                        throw new Exception("BaseUrl and ApiKey cannot be empty for Sonarr HttpClient.");
                    }

                    if (Uri.TryCreate(settings.BaseUrl.TrimEnd('/'), UriKind.Absolute, out var baseUri))
                        client.BaseAddress = baseUri;

                    client.Timeout = TimeSpan.FromSeconds(15);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    if (!string.IsNullOrWhiteSpace(settings.ApiKey))
                        client.DefaultRequestHeaders.Add("X-Api-Key", settings.ApiKey);
                }
            )
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
    }

    public static HttpClient CreateSonarrHttpClient(this IHttpClientFactory httpClientFactory) =>
        httpClientFactory.CreateClient(SonarrClientName);
}
