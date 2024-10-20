using System.Reflection;
using System.Text.Json.Serialization;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Environment;
using FastEndpoints;
using FastEndpoints.Swagger;
using Logging.Interface;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using PlexRipper.Application;
using PlexRipper.Domain.Config;
using Serilog;

namespace PlexRipper.WebAPI;

/// <summary>
///  The Startup class configures the application services and the HTTP request pipeline.
/// </summary>
public static class Startup
{
    /// <summary>
    ///  The CORS Configuration name.
    /// </summary>
    public const string CORSConfiguration = "CORS_Configuration";

    private static readonly ILog _log = LogManager.CreateLogInstance(typeof(Startup));

    /// <summary>
    ///  This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="builder"></param>
    public static void ConfigureHostBuilder(this IHostBuilder builder)
    {
        // Use Autofac as the DI container
        builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        builder.ConfigureContainer<ContainerBuilder>(containerBuilder =>
        {
            _log.DebugLine("Setting up Autofac Containers");
            ContainerConfig.ConfigureContainer(containerBuilder);
        });

        // Add services to the container.
        builder.UseSerilog(LogConfig.GetLogger());
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    /// <param name="app"> The <see cref="IApplicationBuilder"/> instance to configure.</param>
    /// <param name="env"> The <see cref="IWebHostEnvironment"/> instance to configure.</param>
    public static void ConfigureApplication(this WebApplication app, IWebHostEnvironment env)
    {
        _log.Information("Currently running in {Environment} mode", env.EnvironmentName);

        _log.Information(
            "Running location: {Location}",
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
        );

        // This has to always be first
        app.UseCors(CORSConfiguration);

        app.UseRouting();

        app.UseAuthorization();

        if (!EnvironmentExtensions.IsIntegrationTestMode())
        {
            // SignalR configuration
            app.MapHub<ProgressHub>("/progress");
            app.MapHub<NotificationHub>("/notifications");
        }

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
                return result;
            };
        });

        // Setup FastEndpoints Swagger
        app.UseSwaggerGen();

        if (!EnvironmentExtensions.IsIntegrationTestMode() && env.IsProduction())
        {
            // Used to deploy the front-end Nuxt client
            app.UseSpaStaticFiles();
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
            });
        }
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="services"> The <see cref="IServiceCollection"/> instance to configure.</param>
    /// <param name="env"> The <see cref="IWebHostEnvironment"/> instance to configure.</param>
    public static void ConfigureServices(this IServiceCollection services, IWebHostEnvironment env)
    {
        // This has to always be first
        services.AddCors(options =>
        {
            options.AddPolicy(
                CORSConfiguration,
                builder =>
                {
                    builder
                        .SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
            );
        });

        services.AddOptions();

        services.AddAuthorization();

        // Setup FastEndpoints
        services.AddFastEndpoints(options =>
        {
            options.DisableAutoDiscovery = true;
            options.Assemblies = [Assembly.GetAssembly(typeof(BaseEndpoint<,>))!];
        });

        if (!EnvironmentExtensions.IsIntegrationTestMode())
        {
            // Used to deploy the front-end Nuxt client
            if (env.IsProduction())
            {
                services.AddSpaStaticFiles(configuration =>
                    configuration.RootPath = Path.Combine(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "",
                        "wwwroot"
                    )
                );
            }

            if (env.IsDevelopment())
                services.AddSpaStaticFiles(configuration => configuration.RootPath = "ClientApp");

            // Setup SignalR
            services
                .AddSignalR()
                .AddJsonProtocol(options =>
                    options.PayloadSerializerOptions = DefaultJsonSerializerOptions.ConfigStandard
                );

            services.SwaggerDocument(options =>
            {
                // https://fast-endpoints.com/docs/swagger-support#swagger-operation-tags
                options.AutoTagPathSegmentIndex = 2;

                // https://fast-endpoints.com/docs/swagger-support#short-schema-names
                options.ShortSchemaNames = true;

                // https://fast-endpoints.com/docs/swagger-support#removing-empty-schema
                options.RemoveEmptyRequestSchema = true;

                options.AutoTagPathSegmentIndex = 2;

                options.FlattenSchema = true;

                options.SerializerSettings = serializerOptions =>
                {
                    var config = DefaultJsonSerializerOptions.ConfigStandard;
                    serializerOptions.PropertyNameCaseInsensitive = config.PropertyNameCaseInsensitive;
                    serializerOptions.PropertyNamingPolicy = config.PropertyNamingPolicy;
                    serializerOptions.DefaultIgnoreCondition = config.DefaultIgnoreCondition;

                    // This will ensure that Enums are not exported as integers but as string values in the Swagger UI
                    serializerOptions.Converters.Add(new JsonStringEnumConverter());
                };

                // options.ExcludeNonFastEndpoints = true;
                options.DocumentSettings = s =>
                {
                    s.Title = "[FastEndpoints] PlexRipper Swagger Internal API";
                    s.Version = "v1";
                    s.MarkNonNullablePropsAsRequired();
                    s.RequireParametersWithoutDefault = true;
                    s.DocumentProcessors.Add(new NSwagAddExtraTypes());
                };
            });
        }

        services
            .AddHttpClient("")
            .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
            {
                // TODO: Disable SSL Check, might be bad
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
            });

        // Removing all registered IHttpMessageHandlerBuilderFilter instances to disable built-in HttpClient logging
        services.RemoveAll<IHttpMessageHandlerBuilderFilter>();
    }
}
