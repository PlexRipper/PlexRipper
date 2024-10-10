using System.Collections.Specialized;
using Application.Contracts;
using Autofac;
using Autofac.Extras.Quartz;
using Data.Contracts;
using FileSystem.Contracts;
using PlexRipper.Application;
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

        builder.RegisterType<MockSignalRService>().As<ISignalRService>().SingleInstance();

        SetMockedDependencies(builder);

        // Register Quartz dependencies
        builder.RegisterModule(
            new QuartzAutofacFactoryModule { ConfigurationProvider = _ => QuartzModule.TestQuartzConfiguration() }
        );
    }

    private void SetMockedDependencies(ContainerBuilder builder)
    {
        if (MockPlexApi is not null)
            builder.Register((_, _) => MockPlexApi.CreateClient()).As<HttpClient>().InstancePerDependency();

        if (Config.MockFileSystem is not null)
            builder.RegisterInstance(Config.MockFileSystem).As<IFileSystem>();

        if (Config.MockConfigManager is not null)
            builder.RegisterInstance(Config.MockConfigManager).As<IConfigManager>();
    }
}
