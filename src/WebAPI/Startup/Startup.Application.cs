using System.Reflection;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Http.Extensions;
using Reaparr.Environment;

namespace Reaparr.WebAPI;

public static partial class Startup
{
    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    /// <param name="app"> The <see cref="IApplicationBuilder"/> instance to configure.</param>
    /// <param name="env"> The <see cref="IWebHostEnvironment"/> instance to configure.</param>
    public static void ConfigureApplication(this WebApplication app, IWebHostEnvironment env)
    {
        _log.Here()
            .Information(
                "Running location: {Location}",
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            );

        // This has to always be first
        app.UseCors(CORSConfiguration);

        app.UseRouting();

        if (!EnvironmentExtensions.IsIntegrationTestMode())
        {
            // SignalR configuration
            app.MapHub<ProgressHub>("/progress");
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

        app.UseAuthentication();
        app.UseAuthorization();

        // Setup FastEndpoints
        app.UseFastEndpoints(c =>
        {
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
