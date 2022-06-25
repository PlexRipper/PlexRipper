using System;
using Autofac;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using PlexRipper.Application;
using PlexRipper.BaseTests.Config;
using PlexRipper.DownloadManager;
using PlexRipper.WebAPI.Common;

namespace PlexRipper.BaseTests
{
    public class PlexRipperWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly string _memoryDbName;

        private readonly UnitTestDataConfig _config;

        public PlexRipperWebApplicationFactory(string memoryDbName, [CanBeNull] Action<UnitTestDataConfig> options = null)
        {
            _memoryDbName = memoryDbName;
            _config = UnitTestDataConfig.FromOptions(options);
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            return PlexRipperHost.Setup();
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            // source: https://github.com/autofac/Autofac/issues/1207#issuecomment-850405602
            builder
                .ConfigureContainer<ContainerBuilder>(autoFacBuilder =>
                {
                    autoFacBuilder
                        .Register((_, _) => MockDatabase.GetMemoryDbContext(_memoryDbName))
                        .InstancePerDependency();

                    autoFacBuilder.RegisterModule<TestModule>();

                    SetMockedDependencies(autoFacBuilder);

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

        private void SetMockedDependencies(ContainerBuilder builder)
        {
            builder.RegisterType<MockSignalRService>().As<ISignalRService>();

            if (_config.MockFileSystem is not null)
            {
                builder.RegisterInstance(_config.MockFileSystem).As<IFileSystem>();
            }

            if (_config.MockDownloadSubscriptions is not null)
            {
                builder.RegisterInstance(_config.MockDownloadSubscriptions).As<IDownloadSubscriptions>();
            }

            if (_config.MockConfigManager is not null)
            {
                builder.RegisterInstance(_config.MockConfigManager).As<IConfigManager>();
            }
        }
    }
}