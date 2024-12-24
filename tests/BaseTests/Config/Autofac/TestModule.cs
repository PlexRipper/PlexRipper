using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Application.Contracts;
using Autofac;
using Autofac.Extras.Quartz;
using ByteSizeLib;
using Data.Contracts;
using Environment;
using PlexRipper.Application;
using PlexRipper.Data;
using PlexRipper.Identity;
using PlexRipper.Identity.Contracts;
using Settings.Contracts;

namespace PlexRipper.BaseTests;

/// <summary>
/// Add the default test mock modules here which can later be overridden
/// </summary>
public class TestModule : Module
{
    public required string MemoryDbName { get; init; }
    public required UnitTestDataConfig Config { get; init; }

    protected override void Load(ContainerBuilder builder)
    {
        // Database context can be setup once and then retrieved by its DB name.
        builder
            .Register((_, _) => MockDatabase.GetMemoryPlexRipperDbContext(MemoryDbName))
            .As<PlexRipperDbContext>()
            .InstancePerDependency();

        builder
            .Register((_, _) => MockDatabase.GetMemoryPlexRipperDbContext(MemoryDbName))
            .As<IPlexRipperDbContext>()
            .InstancePerDependency();

        builder
            .Register((_, _) => MockDatabase.GetMemoryAuthDbContext(MemoryDbName))
            .As<AuthDbContext>()
            .InstancePerDependency();

        builder
            .Register((_, _) => MockDatabase.GetMemoryAuthDbContext(MemoryDbName))
            .As<IAuthDbContext>()
            .InstancePerDependency();

        builder.RegisterType<TestStreamTracker>().As<ITestStreamTracker>().SingleInstance();
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
        if (Config.HttpClientOptions is not null)
        {
            builder
                .Register(_ =>
                {
                    var handler = new Mock<HttpMessageHandler>();
                    Config.HttpClientOptions.Invoke(handler);
                    var client = new HttpClient(handler.Object);
                    client.DefaultRequestHeaders.Add("User-Agent", "MockHttpClient");
                    return client;
                })
                .As<HttpClient>()
                .InstancePerDependency();
        }

        if (Config.MockConfigManager is not null)
            builder.RegisterInstance(Config.MockConfigManager).As<IConfigManager>();

        // Note: This has to stay outside of scope otherwise Config.FileSystemOptions is not applied when dependency injected
        var fileSystem = new MockFileSystem();
        builder
            .Register<MockFileSystem>(ctx =>
            {
                fileSystem.AddDrive(
                    "/",
                    new MockDriveData()
                    {
                        IsReady = true,
                        DriveType = DriveType.Fixed,
                        AvailableFreeSpace = (long)ByteSize.FromGigaBytes(1000).Bytes,
                    }
                );
                fileSystem.AddDirectory(PathProvider.ConfigDirectory);
                fileSystem.AddDirectory(PathProvider.DefaultDownloadsDestinationFolder);
                fileSystem.AddDirectory(PathProvider.DefaultMovieDestinationFolder);
                fileSystem.AddDirectory(PathProvider.DefaultTvShowsDestinationFolder);
                fileSystem.AddDirectory(PathProvider.DefaultMusicDestinationFolder);
                fileSystem.AddDirectory(PathProvider.DefaultPhotosDestinationFolder);
                fileSystem.AddDirectory(PathProvider.DefaultOtherDestinationFolder);
                fileSystem.AddDirectory(PathProvider.DefaultGamesDestinationFolder);

                var dbContext = ctx.Resolve<IPlexRipperDbContext>();
                if (Config.FileSystemOptions is not null)
                {
                    Config.FileSystemOptions.Invoke(fileSystem, dbContext);
                }

                return fileSystem;
            })
            .As<IFileSystem>()
            .SingleInstance();
    }
}
