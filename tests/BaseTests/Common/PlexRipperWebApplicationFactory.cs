using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using PlexRipper.WebAPI.Common;

namespace PlexRipper.BaseTests
{
    public class PlexRipperWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly UnitTestDataConfig _config;

        public PlexRipperWebApplicationFactory(UnitTestDataConfig config = null)
        {
            _config = config ?? new UnitTestDataConfig();
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            return PlexRipperHost.Setup().ConfigureWebHost(builder => builder.UseEnvironment("Integration Testing"));
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder
                .ConfigureContainer<ContainerBuilder>(autoFacBuilder =>
                {
                    autoFacBuilder.Register((_, _) => MockDatabase
                            .GetMemoryDbContext(_config.MemoryDbName)
                            .Setup(_config))
                        .InstancePerLifetimeScope();

                    //     // SignalR requires the default ILogger
                    //     autoFacBuilder.RegisterInstance(new LoggerFactory()).As<ILoggerFactory>();
                    //     autoFacBuilder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();
                });
            return base.CreateHost(builder);
        }
    }
}