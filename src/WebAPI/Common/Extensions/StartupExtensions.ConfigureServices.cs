using System.Reflection;
using Environment;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using NJsonSchema;
using NSwag.Generation.Processors.Security;
using PlexRipper.Application;
using PlexRipper.Domain.Config;

namespace PlexRipper.WebAPI.Common.Extensions;

public static partial class StartupExtensions
{
    public static readonly string CORSConfiguration = "CORS_Configuration";

    public static void SetupConfigureServices(IServiceCollection services, IWebHostEnvironment env)
    {
        services.SetupCors();

        services.SetupControllers();

        services.SetupFrontEnd(env);

        services.SetupSignalR();

        if (!EnvironmentExtensions.IsIntegrationTestMode())
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
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "wwwroot"; });

        if (env.IsDevelopment())
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp"; });
    }

    public static void SetupControllers(this IServiceCollection services)
    {
        // Controllers and Json options
        services
            .AddControllers();
            // TODO This breaks WebApplication factory by receiving empty JSON body in API integration testing
           // .AddJsonOptions(JsonSerializerOptionsWebApi.Config);

        // Customise default API behaviour
        services.AddHttpContextAccessor();

        // Fluent Validator
        services.AddMvc()
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

    public static void SetupSignalR(this IServiceCollection services)
    {
        // SignalR
        services
            .AddSignalR()
            .AddJsonProtocol(options => options.PayloadSerializerOptions = DefaultJsonSerializerOptions.ConfigBase);
    }

    public static void SetupOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerDocument(configure =>
        {
            configure.GenerateEnumMappingDescription = true;
            configure.Title = "PlexRipper Swagger API";

            // This disables Newtonsoft and enables System.Text.Json
            // configure.SerializerSettings = null;
            // configure.SerializerOptions = DefaultJsonSerializerOptions.ConfigBase;

            // Automatic makes each property required, this avoids unnecessary nullable types for Typescript classes
            configure.SchemaType = SchemaType.Swagger2;

            configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));

            // Unreferenced DTO's in the API can be added here such that the front-end can generate Typescript class from it.
            // Useful for SignalR types
            configure.DocumentProcessors.Add(new NSwagAddExtraTypes());
        });
    }
}