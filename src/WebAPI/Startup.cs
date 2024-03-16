using Logging.Interface;

namespace PlexRipper.WebAPI;

/// <summary>
/// The application startUp class.
/// </summary>
public sealed class Startup
{
    private static readonly ILog _log = LogManager.CreateLogInstance(typeof(Startup));

    /// <summary>
    /// Initializes a new instance of the <see cref="Startup"/> class.
    /// </summary>
    /// <param name="env">Provides information about the web hosting environment an application is running in.</param>
    public Startup(IWebHostEnvironment env)
    {
        CurrentEnvironment = env;
        _log.Information("PlexRipper running in {Environment} mode", CurrentEnvironment.EnvironmentName);
    }

    private IWebHostEnvironment CurrentEnvironment { get; }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="services"></param>
    public void ConfigureServices(IServiceCollection services) { }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> instance to configure.</param>
    public void Configure(IApplicationBuilder app) { }
}