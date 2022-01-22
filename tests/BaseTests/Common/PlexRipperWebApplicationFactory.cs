using System.Threading.Tasks;
using Autofac;
using Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using PlexRipper.Application;
using PlexRipper.BaseTests.Config;
using PlexRipper.DownloadManager;
using PlexRipper.WebAPI.Common;
using PlexRipper.WebAPI.Config;

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

        // protected override IWebHostBuilder CreateWebHostBuilder()
        // {
        //     return base.CreateWebHostBuilder().ConfigureTestContainer<ContainerBuilder>(autoFacBuilder =>
        //     {
        //         Log.Debug("Setting up Autofac Containers");
        //         ContainerConfig.ConfigureContainer(autoFacBuilder);
        //
        //         autoFacBuilder
        //             .Register((_, _) => MockDatabase.GetMemoryDbContext(_config.MemoryDbName))
        //             .InstancePerDependency();
        //
        //         autoFacBuilder.RegisterModule<TestModule>();
        //
        //         SetMockedDependancies(autoFacBuilder);
        //
        //         //  SignalR requires the default ILogger
        //         //  autoFacBuilder.RegisterInstance(new LoggerFactory()).As<ILoggerFactory>();
        //         //  autoFacBuilder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();
        //     });
        // }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            // source: https://github.com/autofac/Autofac/issues/1207#issuecomment-850405602
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

            // Source: https://www.strathweb.com/2021/05/the-curious-case-of-asp-net-core-integration-test-deadlock/
            // var host = builder.Build();
            // Task.Run(() => host.StartAsync()).GetAwaiter().GetResult();
            // return host;

            return base.CreateHost(builder);
        }

        private void SetMockedDependancies(ContainerBuilder builder)
        {
            if (_config.MockFileSystem is not null)
            {
                builder.RegisterInstance(_config.MockFileSystem).As<IFileSystem>();
            }

            if (_config.MockDownloadSubscriptions is not null)
            {
                builder.RegisterInstance(_config.MockDownloadSubscriptions).As<IDownloadSubscriptions>();
            }
        }
    }
}