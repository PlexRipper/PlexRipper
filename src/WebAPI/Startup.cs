using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Autofac;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
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

namespace PlexRipper.WebAPI
{
    /// <summary>
    /// The application startUp class.
    /// </summary>
    public class Startup
    {
        readonly string CORSConfiguration = "CORS_Configuration";

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">Provides information about the web hosting environment an application is running in.</param>
        public Startup(IWebHostEnvironment env)
        {
            CurrentEnvironment = env;
            Log.Information($"PlexRipper running in {CurrentEnvironment.EnvironmentName} mode.");
        }

        private IWebHostEnvironment CurrentEnvironment { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Set CORS Configuration
            services.AddCors(options =>
            {
                options.AddPolicy(
                    CORSConfiguration,
                    builder =>
                    {
                        // TODO CORS disabled, otherwise its not working when deployed in a docker container
                        // Solution?
                        builder
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            // The combo all origin is allowed with allow credentials is needed to make SignalR work from the client.
                            .SetIsOriginAllowed(x => true)
                            .AllowCredentials();
                    });
            });

            // Controllers and Json options
            services
                .AddControllers()
                .AddJsonOptions(
                    options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });


            if (CurrentEnvironment.IsProduction())
            {
                services.AddSpaStaticFiles(configuration => { configuration.RootPath = "wwwroot"; });
            }

            if (CurrentEnvironment.IsDevelopment())
            {
                services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp"; });
            }

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
            services.AddHttpClient();
            services.AddOptions();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance to configure.</param>
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseCors(CORSConfiguration);

            app.UseAuthorization();

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

            if (CurrentEnvironment.IsProduction())
            {
                app.UseSpaStaticFiles();
                app.UseSpa(spa => { spa.Options.SourcePath = "ClientApp"; });
            }

            // if (CurrentEnvironment.IsDevelopment())
            // {
            //     app.UseSpaStaticFiles();
            //     app.UseSpa(spa =>
            //     {
            //         spa.Options.SourcePath = "ClientApp";
            //         spa.Options.DevServerPort = 3000;
            //         spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");
            //     });
            // }
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            Log.Debug("Setting up Autofac Containers");
            ContainerConfig.ConfigureContainer(builder);
            Log.Debug("Finished setting up Autofac Containers");
        }
    }
}