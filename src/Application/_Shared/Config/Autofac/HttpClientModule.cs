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
                    if (string.IsNullOrWhiteSpace(settings.SonarrBaseUrl))
                        throw new InvalidOperationException("Invalid SonarrBaseUrl: value is null or empty.");

                    if (string.IsNullOrWhiteSpace(settings.SonarrApiKey))
                        throw new InvalidOperationException("Invalid SonarrApiKey: value is null or empty.");

                    var baseUrl = settings.SonarrBaseUrl.Trim().TrimEnd('/');
                    if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
                        throw new InvalidOperationException(
                            $"Invalid SonarrBaseUrl: '{settings.SonarrBaseUrl}' is not a valid absolute URI."
                        );
                    client.BaseAddress = baseUri;

                    client.Timeout = TimeSpan.FromSeconds(15);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    if (!string.IsNullOrWhiteSpace(settings.SonarrApiKey))
                        client.DefaultRequestHeaders.Add("X-Api-Key", settings.SonarrApiKey);
                }
            )
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
    }

    public static HttpClient CreateSonarrHttpClient(this IHttpClientFactory httpClientFactory) =>
        httpClientFactory.CreateClient(SonarrClientName);
}
