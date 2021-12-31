using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using PlexRipper.Application;
using PlexRipper.BaseTests.Config;
using PlexRipper.Data;
using PlexRipper.WebAPI.Common;

namespace PlexRipper.BaseTests
{
    public class PlexRipperWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly UnitTestDataConfig _config;

        private readonly PlexRipperDbContext _dbContext;

        public PlexRipperWebApplicationFactory(PlexRipperDbContext dbContext, UnitTestDataConfig config = null)
        {
            _config = config ?? new UnitTestDataConfig();
            _dbContext = dbContext;
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
                    autoFacBuilder
                        .Register((_, _) => MockDatabase.GetMemoryDbContext(_config.MemoryDbName))
                        .InstancePerDependency();

                    autoFacBuilder.RegisterModule<TestModule>();

                    SetMockedDependancies(autoFacBuilder);

                    //  SignalR requires the default ILogger
                    //  autoFacBuilder.RegisterInstance(new LoggerFactory()).As<ILoggerFactory>();
                    //  autoFacBuilder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();
                });
            return base.CreateHost(builder);
        }

        private void SetMockedDependancies(ContainerBuilder builder)
        {
            if (_config.MockFileSystem is not null)
            {
                builder.RegisterInstance(_config.MockFileSystem).As<IFileSystem>();
            }
        }
    }
}