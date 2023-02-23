using System.Reflection;
using System.Text.Json.Serialization;
using Environment;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
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
            .AddControllers()
            .AddJsonOptions(options =>
            {
                var config = DefaultJsonSerializerOptions.ConfigBase;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = config.PropertyNameCaseInsensitive;
                options.JsonSerializerOptions.PropertyNamingPolicy = config.PropertyNamingPolicy;
                options.JsonSerializerOptions.DefaultIgnoreCondition = config.DefaultIgnoreCondition;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

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
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "PlexRipper Swagger Internal API",
            });
            options.SchemaGeneratorOptions.SupportNonNullableReferenceTypes = true;
            options.SchemaFilter<RequiredMemberFilter>();
            options.SchemaFilter<RequiredNotNullableSchemaFilter>();
            options.AddSignalRSwaggerGen();
        });
    }
}