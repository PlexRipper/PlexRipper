using System.Reflection;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Http.Extensions;

namespace Reaparr.AppHost;

public static partial class Startup
{
    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    /// <param name="app"> The <see cref="IApplicationBuilder"/> instance to configure.</param>
    /// <param name="env"> The <see cref="IWebHostEnvironment"/> instance to configure.</param>
    /// <param name="appBuildInfo"> The <see cref="IAppBuildInfo"/> instance containing application build information to include in response headers.</param>
    public static void ConfigureApplication(this WebApplication app, IWebHostEnvironment env, IAppBuildInfo appBuildInfo)
    {
        _log.Here()
            .Information(
                "Running location: {Location}",
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            );

        // Swallows OperationCanceledException caused by client-aborted requests (AbortController).
        // Must be outermost so it wraps all downstream middleware including CORS and FastEndpoints.
        app.UseMiddleware<RequestCancellationMiddleware>();

        // This has to always be first
        app.UseCors(CorsConfiguration);
            
        app.Use(
            async (ctx, next) =>
            {
                // Rewrite legacy/public API v2 routes
                if (ctx.Request.Path.StartsWithSegments("/api/v2", out var remaining))
                    ctx.Request.Path = PublicApiRoutes.DownloadClient + remaining;

                // Set global response headers
                ctx.Response.OnStarting(() =>
                {
                    // NOTE: Update "NSwagGlobalHeaders" when adding/updating headers and add to "CORS WithExposedHeaders" in ConfigureServices
                    ctx.Response.Headers[ReaparrHeaders.Version] = appBuildInfo.GetInformationalVersion;
                    ctx.Response.Headers[ReaparrHeaders.Platform] = appBuildInfo.GetRuntimeMode;

                    return Task.CompletedTask;
                });
                
                await next();
            }
        );

        app.UseRouting();

        if (!EnvironmentExtensions.IsIntegrationTestMode())
        {
            // SignalR configuration
            app.MapHub<LogHub>("/logs");
            app.MapHub<ProgressHub>("/progress");
            app.MapHub<DownloadHub>("/download");
            app.MapHub<NotificationHub>("/notifications");

            // Place this before app.UseAuthentication().UseAuthorization(); to allow it as anonymous
            app.UseSwaggerGen();
        }

        // Setup FastEndpoints Swagger
        if (!EnvironmentExtensions.IsIntegrationTestMode() && env.IsProduction())
        {
            // Used to deploy the front-end Nuxt client
            app.UseSpaStaticFiles();
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
            });
        }

        // Use custom header authentication middleware
        app.UseMiddleware<HeaderAuthenticationMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        // When I_AM_DUMB_SO_DISABLE_AUTHENTICATION is set, bypass all auth with a synthetic Admin principal.
        // This must run after authentication/authorization so the synthetic principal is not overwritten.
        if (EnvironmentExtensions.IsAuthenticationDisabled())
            app.UseMiddleware<DisableAuthenticationMiddleware>();

        // Enable response caching for downstream caches (must be before FastEndpoints)
        // Doc: https://fast-endpoints.com/docs/response-caching
        app.UseResponseCaching();

        // Set up FastEndpoints
        app.UseFastEndpoints(c =>
        {
            c.Endpoints.Configurator = ep =>
            {
                if (ep.Routes.All(x => x.StartsWith(PublicApiRoutes.Base)))
                    ep.Options(b => b.IsPublicApi());
                else if (ep.Routes.All(x => x.StartsWith(ApiRoutes.Base)))
                    ep.Options(b => b.IsInternalApi());
            };

            // https://fast-endpoints.com/docs/swagger-support#short-endpoint-names
            c.Endpoints.ShortNames = true;

            c.Errors.ResponseBuilder = (failures, ctx, _) =>
            {
                var result = ResultExtensions.Create400BadRequestResult($"Bad request: {ctx.Request.GetDisplayUrl()}");
                var errors = failures
                    .GroupBy(f => f.PropertyName)
                    .ToDictionary(e => e.Key, e => e.Select(m => m.ErrorMessage).ToArray());
                foreach (var reason in errors)
                    result.Errors[0].Metadata.Add(reason.Key, reason.Value);

                return result.LogError();
            };
        });
    }
}
