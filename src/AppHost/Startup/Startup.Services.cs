using System.Reflection;
using System.Text.Json.Serialization;
using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using MessagePack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using NSwag;
using NSwag.Generation.Processors.Security;
using Reaparr.Application;
using Reaparr.Application.Contracts;
using Reaparr.Environment;
using Reaparr.Identity;
using Reaparr.Identity.Contracts;
using Reaparr.PlexApi;

namespace Reaparr.AppHost;

/// <summary>
///  The Startup class configures the application services and the HTTP request pipeline.
/// </summary>
public static partial class Startup
{
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
                        .AllowCredentials()
                        .WithExposedHeaders("X-Reaparr-Version");
                }
            );
        });

        services.AddOptions();

        services.AddHttpContextAccessor();

        services.ConfigureAuthenticationServices();

        // Setup FastEndpoints
        services.AddFastEndpoints(options =>
        {
            // Manually define the assemblies to scan for FastEndpoints
            options.DisableAutoDiscovery = true;
            options.Assemblies =
            [
                // Reference the assemblies that contain the FastEndpoints or ICommand implementations
                Assembly.GetAssembly(typeof(ApplicationModule))!,
                Assembly.GetAssembly(typeof(PlexApiModule))!,
            ];
        });

        services.AddCommandMiddleware(c => c.Register(typeof(ValidationPipeline<,>)));

        if (!EnvironmentExtensions.IsIntegrationTestMode())
        {
            // Used to deploy the front-end Nuxt client
            if (env.IsProduction())
            {
                var path = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "",
                    "wwwroot"
                );
                _log.Here().Debug("Setting up SPA static files for production at {Path}", path);
                services.AddSpaStaticFiles(configuration => configuration.RootPath = path);
            }

            if (env.IsDevelopment())
                services.AddSpaStaticFiles(configuration => configuration.RootPath = "ClientApp");

            // Setup SignalR
            services
                .AddSignalR()
                .AddJsonProtocol(options =>
                    options.PayloadSerializerOptions = DefaultJsonSerializerOptions.ConfigStandard
                )
                .AddMessagePackProtocol(options =>
                {
                    options.SerializerOptions = MessagePackSerializerOptions.Standard.WithSecurity(
                        MessagePackSecurity.UntrustedData
                    );
                });

            services.SwaggerDocument(options =>
            {
                // https://fast-endpoints.com/docs/swagger-support#swagger-operation-tags
                options.AutoTagPathSegmentIndex = 2;

                // https://fast-endpoints.com/docs/swagger-support#short-schema-names
                options.ShortSchemaNames = true;

                options.EnableJWTBearerAuth = false;

                // https://fast-endpoints.com/docs/swagger-support#removing-empty-schema
                options.RemoveEmptyRequestSchema = true;

                options.AutoTagPathSegmentIndex = 2;

                options.FlattenSchema = true;

                options.EndpointFilter = ep => !ep.EndpointType.IsDefined(typeof(HideFromOpenApiAttribute));

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
                    s.Title = "Reaparr Internal API  (NOT FOR EXTERNAL USE)";
                    s.Version = "v1";

                    s.MarkNonNullablePropsAsRequired();

                    s.DocumentProcessors.Add(new NSwagAddExtraTypes());
                    s.OperationProcessors.Add(new NSwagGlobalHeaders());

                    // Add cookie-based authentication
                    s.AddAuth(
                        "CookieAuth",
                        new()
                        {
                            Type = OpenApiSecuritySchemeType.ApiKey,
                            In = OpenApiSecurityApiKeyLocation.Cookie,
                            Name = DefaultUserAppCredentials.DefaultCookieName,
                            Description = "Cookie-based authentication for the internal Reaparr API",
                        }
                    );

                    // 🔹 Header-based authentication
                    s.AddAuth(
                        "HeaderAuth",
                        new()
                        {
                            Type = OpenApiSecuritySchemeType.ApiKey,
                            In = OpenApiSecurityApiKeyLocation.Header,
                            Name = EnvironmentExtensions.GetHeaderAuthTokenName(),
                            Description = "Header-based authentication via trusted proxy",
                        }
                    );
                    s.OperationProcessors.Add(new OperationSecurityScopeProcessor("HeaderAuth"));

                    s.OperationProcessors.Add(new OperationSecurityScopeProcessor("CookieAuth"));
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

    private static void ConfigureAuthenticationServices(this IServiceCollection services)
    {
        services.AddDataProtection().PersistKeysToDbContext<AuthDbContext>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AuthenticatedUsers", x => x.RequireRole("Admin"));

            // Set a default policy that requires authentication
            options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        });

        services
            .AddIdentityApiEndpoints<AppUser>(options =>
            {
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AuthDbContext>();

        services.AddAuthenticationCookie(
            validFor: TimeSpan.FromHours(6),
            c =>
            {
                c.Cookie.Name = DefaultUserAppCredentials.DefaultCookieName;
                c.LoginPath = ApiRoutes.LoginEndpoint;
                c.LogoutPath = ApiRoutes.LogOutEndpoint;
                c.SlidingExpiration = true;
                c.AccessDeniedPath = "/api/access-denied";

                c.Events.OnRedirectToLogin = ctx =>
                {
                    if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                        ctx.Response.StatusCode = 401;

                    return Task.CompletedTask;
                };
                c.Events.OnRedirectToAccessDenied = ctx =>
                {
                    if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                        ctx.Response.StatusCode = 403;

                    return Task.CompletedTask;
                };
            }
        );
    }
}
