using System.Reflection;
using System.Text.Json.Serialization;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Environment;
using FastEndpoints;
using FastEndpoints.Swagger;
using Logging.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using PlexRipper.Application;
using PlexRipper.Data;
using PlexRipper.Domain.Config;
using PlexRipper.WebAPI.Common;
using Serilog;
using Serilog.Events;

namespace PlexRipper.WebAPI;

public static class Program
{
    private static readonly ILog _log = LogManager.CreateLogInstance(typeof(PlexRipperHost));

    public static void Main(string[] args)
    {
        try
        {
            LogManager.SetupLogging(LogEventLevel.Verbose);
            _log.Information("Currently running on {CurrentOS}", OsInfo.CurrentOS);

            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog(LogConfig.GetLogger());

            builder.WebHost.UseContentRoot(Directory.GetCurrentDirectory());

            ConfigureDatabase();

            // Use Autofac as the DI container
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(containerBuilder =>
                {
                    _log.DebugLine("Setting up Autofac Containers");
                    ContainerConfig.ConfigureContainer(containerBuilder);
                });

            // Setup the services
            builder.Services.SetupConfigureServices(builder.Environment);
            var app = builder.Build();

            // Setup the app
            app.SetupConfigureApp(builder.Environment);

            app.Run();
        }
        catch (Exception e)
        {
            Result.Fail(new ExceptionalError(e)).LogFatal();
        }
        finally
        {
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            LogManager.CloseAndFlush();
        }
    }

    public static void ConfigureDatabase()
    {
        var dbContext = new PlexRipperDbContext(new PathProvider());
        dbContext.Setup();
    }

    public static readonly string CORSConfiguration = "CORS_Configuration";

    public static void SetupConfigureApp(this WebApplication app, IWebHostEnvironment env)
    {
        app.UseRouting();

        app.UseCors(CORSConfiguration);

        app.UseAuthorization();
        if (!EnvironmentExtensions.IsIntegrationTestMode())
        {
            // app.UseSwaggerUI(options =>
            // {
            //     options.SwaggerEndpoint("v1/swagger.json", "PlexRipper Swagger API V1");
            //     options.EnableFilter();
            // });
        }

        app.UseFastEndpoints();
        app.UseSwaggerGen();

        // app.UseEndpoints(endpoints =>
        // {
        //     endpoints.MapControllers();
        //     if (!EnvironmentExtensions.IsIntegrationTestMode())
        //     {
        //         endpoints.MapSwagger();
        //
        //         // SignalR configuration
        //         endpoints.MapHub<ProgressHub>("/progress");
        //         endpoints.MapHub<NotificationHub>("/notifications");
        //     }
        // });

        if (!EnvironmentExtensions.IsIntegrationTestMode() && env.IsProduction())
        {
            // Used to deploy the front-end Nuxt client
            app.UseSpaStaticFiles();
            app.UseSpa(spa => { spa.Options.SourcePath = "ClientApp"; });
        }
    }

    public static void SetupConfigureServices(this IServiceCollection services, IWebHostEnvironment env)
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

        services.SetupControllers();

        if (!EnvironmentExtensions.IsIntegrationTestMode())
        {
            // Used to deploy the front-end Nuxt client
            if (env.IsProduction())
                services.AddSpaStaticFiles(configuration => configuration.RootPath = "wwwroot");

            if (env.IsDevelopment())
                services.AddSpaStaticFiles(configuration => configuration.RootPath = "ClientApp");

            // Setup SignalR
            services
                .AddSignalR()
                .AddJsonProtocol(options => options.PayloadSerializerOptions = DefaultJsonSerializerOptions.ConfigBase);

            // services.AddSwaggerGen(c =>
            // {
            //     c.SwaggerDoc("v1", new OpenApiInfo
            //     {
            //         Version = "v1",
            //         Title = "PlexRipper Swagger Internal API",
            //     });
            //     c.SchemaGeneratorOptions.SupportNonNullableReferenceTypes = true;
            //     c.SchemaFilter<RequiredMemberFilter>();
            //     c.SchemaFilter<RequiredNotNullableSchemaFilter>();
            //     c.AddSignalRSwaggerGen();
            //
            //     // Enables the XML-documentation for the Swagger UI
            //     c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
            //         $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
            // });
        }

        services.AddOptions();

        // Setup FastEndpoints
        services.AddFastEndpoints(options =>
            {
                options.DisableAutoDiscovery = true;
                options.Assemblies = new[]
                {
                    Assembly.GetAssembly(typeof(BaseCustomEndpoint<,>)),
                };
            })
            .SwaggerDocument(options =>
            {
                options.AutoTagPathSegmentIndex = 2;

                // options.ExcludeNonFastEndpoints = true;
                options.DocumentSettings = s =>
                {
                    s.Title = "[FastEndpoints] PlexRipper Swagger Internal API";
                    s.Version = "v1";
                };
            });

        services.AddHttpClient();

        // Removing all registered IHttpMessageHandlerBuilderFilter instances to disable built-in HttpClient logging
        services.RemoveAll<IHttpMessageHandlerBuilderFilter>();
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

        // https://autofac.readthedocs.io/en/latest/integration/aspnetcore.html#controllers-as-services
        services.AddMvc().AddControllersAsServices();

        services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });
    }
}