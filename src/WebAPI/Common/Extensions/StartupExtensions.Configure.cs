using Environment;
using PlexRipper.WebAPI.SignalR.Hubs;

namespace PlexRipper.WebAPI.Common.Extensions;

public static partial class StartupExtensions
{
    public static void SetupConfigure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        app.UseCors(CORSConfiguration);

        app.UseAuthorization();
        if (!EnvironmentExtensions.IsIntegrationTestMode())
        {
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("v1/swagger.json", "PlexRipper Swagger API V1");
                options.EnableFilter();
            });
        }

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            if (!EnvironmentExtensions.IsIntegrationTestMode())
            {
                endpoints.MapSwagger();

                // SignalR configuration
                endpoints.MapHub<ProgressHub>("/progress");
                endpoints.MapHub<NotificationHub>("/notifications");
            }
        });

        if (!EnvironmentExtensions.IsIntegrationTestMode() && env.IsProduction())
        {
            // Used to deploy the front-end Nuxt client
            app.UseSpaStaticFiles();
            app.UseSpa(spa => { spa.Options.SourcePath = "ClientApp"; });
        }
    }
}