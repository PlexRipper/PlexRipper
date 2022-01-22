using Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlexRipper.WebAPI.Common.Extensions;

namespace PlexRipper.WebAPI
{
    /// <summary>
    /// The application startUp class.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">Provides information about the web hosting environment an application is running in.</param>
        public Startup(IWebHostEnvironment env)
        {
            CurrentEnvironment = env;
            Log.Information($"PlexRipper running in {CurrentEnvironment.EnvironmentName ?? "Unknown"} mode.");
        }

        protected IWebHostEnvironment CurrentEnvironment { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public virtual void ConfigureServices(IServiceCollection services)
        {
            StartupExtensions.SetupConfigureServices(services, CurrentEnvironment);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance to configure.</param>
        public virtual void Configure(IApplicationBuilder app)
        {
            StartupExtensions.SetupConfigure(app, CurrentEnvironment);
        }
    }
}