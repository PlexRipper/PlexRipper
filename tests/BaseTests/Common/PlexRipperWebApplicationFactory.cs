using System.Collections.Specialized;
using Autofac;
using Autofac.Extras.Quartz;
using FileSystem.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using PlexRipper.Application;
using PlexRipper.Data;
using PlexRipper.Domain.Autofac;
using PlexRipper.WebAPI.Common;
using Settings.Contracts;
using WebAPI.Contracts;

namespace PlexRipper.BaseTests;

public class PlexRipperWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    private readonly string _memoryDbName;
    private readonly MockPlexApi _mockPlexApi;

    private readonly UnitTestDataConfig _config;

    public PlexRipperWebApplicationFactory(string memoryDbName, Action<UnitTestDataConfig> options = null, MockPlexApi mockPlexApi = null)
    {
        _memoryDbName = memoryDbName;
        _mockPlexApi = mockPlexApi;
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

                    // Database context can be setup once and then retrieved by its DB name.
                    .Register((_, _) => MockDatabase.GetMemoryDbContext(_memoryDbName))
                    .As<PlexRipperDbContext>()
                    .InstancePerDependency();

                autoFacBuilder.RegisterModule<TestModule>();

                SetMockedDependencies(autoFacBuilder);
                RegisterBackgroundScheduler(autoFacBuilder);

                //  SignalR requires the default ILogger
                //  autoFacBuilder.RegisterInstance(new LoggerFactory()).As<ILoggerFactory>();
                //  autoFacBuilder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();
            });

        // Source: https://www.strathweb.com/2021/05/the-curious-case-of-asp-net-core-integration-test-deadlock/
        // var host = builder.Build();
        // Task.Run(() => host.StartAsync()).GetAwaiter().GetResult();
        // return host;

        try
        {
            return base.CreateHost(builder);
        }
        catch (Exception e)
        {
            Log.Fatal(e);
            throw;
        }
    }

    private void SetMockedDependencies(ContainerBuilder builder)
    {
        builder.RegisterType<MockSignalRService>().As<ISignalRService>();

        if (_mockPlexApi is not null)
        {
            builder
                .RegisterInstance(_mockPlexApi.CreateClient())
                .As<System.Net.Http.HttpClient>()
                .SingleInstance();
        }

        if (_config.MockFileSystem is not null)
            builder.RegisterInstance(_config.MockFileSystem).As<IFileSystem>();

        if (_config.MockConfigManager is not null)
            builder.RegisterInstance(_config.MockConfigManager).As<IConfigManager>();
    }

    private void RegisterBackgroundScheduler(ContainerBuilder builder)
    {
        var testQuartzProps = new NameValueCollection
        {
            { "quartz.scheduler.instanceName", "PlexRipper Scheduler" },
            { "quartz.serializer.type", "json" },
            { "quartz.threadPool.type", "Quartz.Simpl.SimpleThreadPool, Quartz" },
            { "quartz.threadPool.threadCount", "10" },
            { "quartz.jobStore.misfireThreshold", "60000" },
        };

        // Register Quartz dependencies
        builder.RegisterModule(new QuartzAutofacFactoryModule
        {
            JobScopeConfigurator = (cb, tag) =>
            {
                // override dependency for job scope
                cb.Register(_ => new ScopedDependency("job-local " + DateTime.UtcNow.ToLongTimeString()))
                    .AsImplementedInterfaces()
                    .InstancePerMatchingLifetimeScope(tag);
            },

            // During integration testing, we cannot use a real JobStore so we revert to default
            ConfigurationProvider = _ => testQuartzProps,
        });
    }
}