using System.Reflection;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using NSwag;
using NSwag.Generation.Processors.Security;
using PlexRipper.Application;
using PlexRipper.WebAPI.Config;
using Quartz;

namespace PlexRipper.WebAPI.Common.Extensions
{
    public static partial class StartupExtensions
    {
        public static readonly string CORSConfiguration = "CORS_Configuration";

        public static void SetupConfigureServices(IServiceCollection services, IWebHostEnvironment env)
        {
            services.SetupCors();

            services.SetupControllers();

            services.SetupFrontEnd(env);
            services.SetupQuartz();

            services.SetupSignalR();

            services.SetupOpenApiDocumentation();
            services.AddOptions();
        }

        public static void SetupTestConfigureServices(IServiceCollection services, IWebHostEnvironment env)
        {
            services.SetupCors();
            services.SetupControllers();
            services.AddOptions();
        }

        public static void SetupCors(this IServiceCollection services)
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
                            .SetIsOriginAllowed(_ => true)
                            .AllowCredentials();
                    });
            });
        }

        public static void SetupFrontEnd(this IServiceCollection services, IWebHostEnvironment env)
        {
            // Used to deploy the front-end Nuxt client
            if (env.IsProduction())
            {
                services.AddSpaStaticFiles(configuration => { configuration.RootPath = "wwwroot"; });
            }

            if (env.IsDevelopment())
            {
                services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp"; });
            }
        }

        public static void SetupControllers(this IServiceCollection services)
        {
            // Controllers and Json options
            services
                .AddControllers()
                .AddJsonOptions(JsonSerializerOptionsWebApi.Config);

            // Customise default API behaviour
            services.AddHttpContextAccessor();

            // Fluent Validator
            services.AddMvc(options => { options.Filters.Add<ValidateFilter>(); })
                .AddFluentValidation(fv =>
                {
                    fv.RegisterValidatorsFromAssemblyContaining<WebApiModule>();
                    fv.RegisterValidatorsFromAssemblyContaining<ApplicationModule>();
                    fv.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
                })

                // https://autofac.readthedocs.io/en/latest/integration/aspnetcore.html#controllers-as-services
                .AddControllersAsServices();

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });
        }

        public static void SetupQuartz(this IServiceCollection services)
        {
            // Quartz setup
            services.AddQuartzHostedService(options =>
            {
                // when shutting down we want jobs to complete gracefully
                options.WaitForJobsToComplete = true;
            });
        }

        public static void SetupSignalR(this IServiceCollection services)
        {
            // SignalR
            services.AddSignalR().AddJsonProtocol(options => { options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
        }

        public static void SetupOpenApiDocumentation(this IServiceCollection services)
        {
            services.AddOpenApiDocument(configure =>
            {
                configure.GenerateEnumMappingDescription = true;
                configure.Title = "PlexRipper API";
                configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "Type into the text box: Bearer {your JWT token}.",
                });
                configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
                configure.DocumentProcessors.Add(new NSwagAddExtraTypes());
            });
        }
    }
}