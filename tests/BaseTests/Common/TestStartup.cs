using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PlexRipper.WebAPI;
using PlexRipper.WebAPI.Common.Extensions;

namespace PlexRipper.BaseTests
{
    public class TestStartup : Startup
    {
        public TestStartup(IWebHostEnvironment env) : base(env) { }

        public override void ConfigureServices(IServiceCollection services)
        {
            StartupExtensions.SetupTestConfigureServices(services);
        }

        public override void Configure(IApplicationBuilder app)
        {
            StartupExtensions.SetupTestConfigure(app);
        }
    }
}