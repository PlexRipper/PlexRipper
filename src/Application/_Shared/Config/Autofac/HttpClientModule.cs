using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application;

public static class HttpClientModule
{
    internal static readonly string SonarrClientName = "Sonarr";
    internal static readonly string RadarrClientName = "Radarr";
    public static readonly string PlexThumbnailClientName = "PlexThumbnail";

    public static void RegisterSonarrHttpClient(this IServiceCollection services)
    {
        services
            .AddHttpClient(
                SonarrClientName,
                (sp, client) =>
                {
                    var settings = sp.GetService<ISonarrSettings>();
                    if (settings == null || string.IsNullOrWhiteSpace(settings.SonarrBaseUrl))
                        return;

                    if (!Uri.TryCreate(settings.SonarrBaseUrl.Trim().TrimEnd('/'), UriKind.Absolute, out var baseUri))
                        return;

                    client.BaseAddress = baseUri;
                    client.Timeout = TimeSpan.FromSeconds(15);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    if (!string.IsNullOrWhiteSpace(settings.SonarrApiKey))
                        client.DefaultRequestHeaders.Add("X-Api-Key", settings.SonarrApiKey);
                }
            )
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
    }

    public static HttpClient CreateSonarrHttpClient(this IHttpClientFactory factory) =>
        factory.CreateClient(SonarrClientName);

    public static void RegisterRadarrHttpClient(this IServiceCollection services)
    {
        services
            .AddHttpClient(
                RadarrClientName,
                (sp, client) =>
                {
                    var settings = sp.GetService<IRadarrSettings>();
                    if (settings == null || string.IsNullOrWhiteSpace(settings.RadarrBaseUrl))
                        return;

                    var normalizedBaseUrl = settings.RadarrBaseUrl.Trim().TrimEnd('/') + "/";
                    if (!Uri.TryCreate(normalizedBaseUrl, UriKind.Absolute, out var baseUri))
                        return;

                    client.BaseAddress = baseUri;
                    client.Timeout = TimeSpan.FromSeconds(15);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    if (!string.IsNullOrWhiteSpace(settings.RadarrApiKey))
                        client.DefaultRequestHeaders.Add("X-Api-Key", settings.RadarrApiKey);
                }
            )
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
    }

    public static HttpClient CreateRadarrHttpClient(this IHttpClientFactory factory) =>
        factory.CreateClient(RadarrClientName);

    public static void RegisterPlexThumbnailHttpClient(this IServiceCollection services)
    {
        services
            .AddHttpClient(
                PlexThumbnailClientName,
                client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/jpeg"));
                }
            )
            .ConfigurePrimaryHttpMessageHandler(() =>
                new SocketsHttpHandler
                {
                    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                    MaxConnectionsPerServer = 10,
                    SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                    {
                        RemoteCertificateValidationCallback = (_, _, _, _) => true,
                    },
                }
            );
    }
}
