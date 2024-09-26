using System.Collections.Specialized;
using Application.Contracts;
using Autofac;
using Autofac.Extras.Quartz;
using Data.Contracts;
using FileSystem.Contracts;
using PlexRipper.Data;
using Settings.Contracts;

namespace PlexRipper.BaseTests;

/// <summary>
/// Add the default test mock modules here which can later be overridden
/// </summary>
public class TestModule : Module
{
    public required string MemoryDbName { get; init; }
    public required MockPlexApi? MockPlexApi { get; init; }
    public required UnitTestDataConfig Config { get; init; }

    protected override void Load(ContainerBuilder builder)
    {
        // Database context can be setup once and then retrieved by its DB name.
        builder
            .Register((_, _) => MockDatabase.GetMemoryDbContext(MemoryDbName))
            .As<PlexRipperDbContext>()
            .InstancePerDependency();

        builder
            .Register((_, _) => MockDatabase.GetMemoryDbContext(MemoryDbName))
            .As<IPlexRipperDbContext>()
            .InstancePerDependency();

        builder.RegisterType<TestStreamTracker>().As<ITestStreamTracker>().SingleInstance();
        builder.RegisterType<MockDownloadFileStream>().As<IDownloadFileStream>().SingleInstance();
        builder.RegisterType<MockFileMergeStreamProvider>().As<IFileMergeStreamProvider>().SingleInstance();
        builder.RegisterType<MockFileMergeSystem>().As<IFileMergeSystem>().SingleInstance();
        builder.RegisterType<MockConfigManager>().As<IConfigManager>().SingleInstance();

        builder.RegisterType<TestLoggingClass>().SingleInstance();

        builder.RegisterType<MockSignalRService>().As<ISignalRService>().SingleInstance();

        SetMockedDependencies(builder);
        RegisterBackgroundScheduler(builder);
    }

    private void SetMockedDependencies(ContainerBuilder builder)
    {
        if (MockPlexApi is not null)
            builder.Register((_, _) => MockPlexApi.CreateClient()).As<HttpClient>().SingleInstance();

        if (Config.MockFileSystem is not null)
            builder.RegisterInstance(Config.MockFileSystem).As<IFileSystem>();

        if (Config.MockConfigManager is not null)
            builder.RegisterInstance(Config.MockConfigManager).As<IConfigManager>();
    }

    private void RegisterBackgroundScheduler(ContainerBuilder builder)
    {
        // During integration testing, we cannot use a real JobStore so we revert to default
        var testQuartzProps = new NameValueCollection
        {
            { "quartz.scheduler.instanceName", "TestPlexRipper Scheduler" },
            { "quartz.serializer.type", "json" },
            { "quartz.threadPool.type", "Quartz.Simpl.SimpleThreadPool, Quartz" },
            { "quartz.threadPool.threadCount", "10" },
            { "quartz.jobStore.misfireThreshold", "60000" },
        };

        // Register Quartz dependencies
        builder.RegisterModule(new QuartzAutofacFactoryModule { ConfigurationProvider = _ => testQuartzProps });
    }
}
