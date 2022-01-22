using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PlexRipper.WebAPI.SignalR.Hubs;

namespace PlexRipper.WebAPI.Common.Extensions
{
    public static partial class StartupExtensions
    {
        public static void SetupConfigure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseCors(CORSConfiguration);

            app.UseAuthorization();

            app.UseOpenApi(); // serve OpenAPI/Swagger documents
            app.UseSwaggerUi3(); // serve Swagger UI
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                // SignalR configuration
                endpoints.MapHub<ProgressHub>("/progress");
                endpoints.MapHub<NotificationHub>("/notifications");
            });

            // Used to deploy the front-end Nuxt client
            if (env.IsProduction())
            {
                app.UseSpaStaticFiles();
                app.UseSpa(spa => { spa.Options.SourcePath = "ClientApp"; });
            }
        }

        public static void SetupTestConfigure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}