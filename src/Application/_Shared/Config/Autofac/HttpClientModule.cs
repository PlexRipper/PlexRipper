using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application;

public static class HttpClientModule
{
    internal static readonly string SonarrClientName = "Sonarr";
    internal static readonly string RadarrClientName = "Radarr";

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

    public static void RegisterRadarrHttpClient(this IServiceCollection services)
    {
        services
            .AddHttpClient(
                RadarrClientName,
                (sp, client) =>
                {
                    var settings = sp.GetRequiredService<IRadarrSettings>();
                    if (string.IsNullOrWhiteSpace(settings.RadarrBaseUrl))
                        throw new InvalidOperationException("Invalid RadarrBaseUrl: value is null or empty.");

                    if (string.IsNullOrWhiteSpace(settings.RadarrApiKey))
                        throw new InvalidOperationException("Invalid RadarrApiKey: value is null or empty.");

                    var baseUrl = settings.RadarrBaseUrl.Trim().TrimEnd('/');
                    if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
                        throw new InvalidOperationException(
                            $"Invalid RadarrBaseUrl: '{settings.RadarrBaseUrl}' is not a valid absolute URI."
                        );
                    client.BaseAddress = baseUri;

                    client.Timeout = TimeSpan.FromSeconds(15);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    if (!string.IsNullOrWhiteSpace(settings.RadarrApiKey))
                        client.DefaultRequestHeaders.Add("X-Api-Key", settings.RadarrApiKey);
                }
            )
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
    }

    public static HttpClient CreateRadarrHttpClient(this IHttpClientFactory httpClientFactory) =>
        httpClientFactory.CreateClient(RadarrClientName);
}
