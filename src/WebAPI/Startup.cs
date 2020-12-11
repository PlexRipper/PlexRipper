using Autofac;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSwag;
using NSwag.Generation.Processors.Security;
using PlexRipper.Application.Config;
using PlexRipper.Domain;
using PlexRipper.SignalR.Hubs;
using PlexRipper.WebAPI.Common;
using PlexRipper.WebAPI.Config;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace PlexRipper.WebAPI
{
    public class Startup
    {
        readonly string CORSConfiguration = "CORS_Configuration";

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // General
            services.AddCors(options =>
            {
                options.AddPolicy(
                    CORSConfiguration,
                    builder =>
                    {
                        builder
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials()
                            .WithOrigins("http://localhost:5000");

                        // .SetPreflightMaxAge(TimeSpan.FromMinutes(100));
                    });
            });
            services.AddControllers().AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "wwwroot"; });

            services.AddHttpContextAccessor();
            services.AddHealthChecks();

            // Fluent Validator
            services.AddMvc(options => { options.Filters.Add<ValidateFilter>(); })
                .AddFluentValidation(fv =>
                {
                    fv.RegisterValidatorsFromAssemblyContaining<ApplicationModule>();
                    fv.RegisterValidatorsFromAssemblyContaining<ValidateFilter>();
                    fv.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
                });
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // SignalR
            services.AddSignalR().AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.Converters
                    .Add(new JsonStringEnumConverter());
            });

            // Customise default API behaviour
            services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });
            services.AddOpenApiDocument(configure =>
            {
                configure.GenerateEnumMappingDescription = true;
                configure.Title = "PlexRipper API";
                configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "Type into the textbox: Bearer {your JWT token}.",
                });
                configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
                configure.DocumentProcessors.Add(new NSwagAddExtraTypes());
            });

            // Autofac
            services.AddOptions();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseCors(CORSConfiguration);
            app.UseCorsMiddleware();
            app.UseAuthorization();

            // Enabling this causes CORS errors as the front-end is in http and cannot connect with an https back-end
            // app.UseHttpsRedirection();
            app.UseOpenApi(); // serve OpenAPI/Swagger documents
            app.UseSwaggerUi3(); // serve Swagger UI
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/api/health");

                // SignalR configuration
                endpoints.MapHub<ProgressHub>("/progress");
                endpoints.MapHub<NotificationHub>("/notifications");
            });

            app.UseSpaStaticFiles();
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.Options.DevServerPort = 3000;
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");

                    // spa.UseVueCli("dev", 3000);
                }
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            Log.Debug("Setting up Autofac Containers");
            ContainerConfig.ConfigureContainer(builder);
            Log.Debug("Finished setting up Autofac Containers");
        }
    }
}
