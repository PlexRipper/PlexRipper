using System.IO.Abstractions.TestingHelpers;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.BaseTests;

public partial class BaseUnitTest
{
    private Action<ContainerBuilder>? _fileSystemSetup;
    private Action<ContainerBuilder>? _httpClientSetup;
    private readonly MockFileSystem _fileSystem = new();
    protected AutoMock Mock { get; set; }

    protected long DefaultAvailableSpace = (long)ByteSize.FromGigaBytes(1000).Bytes;

    private void Build()
    {
        Mock = AutoMock.GetStrict(builder =>
        {
            SetDefaultBuilder(builder);

            if (_fileSystemSetup is not null)
            {
                SetDefaultFileSystemDirectories();
                _fileSystemSetup.Invoke(builder);

                builder.Register(ctx => ctx.Resolve<IFileSystem>().Path).As<IPath>().SingleInstance();
                builder.Register(ctx => ctx.Resolve<IFileSystem>().File).As<IFile>().SingleInstance();
                builder.Register(ctx => ctx.Resolve<IFileSystem>().Directory).As<IDirectory>().SingleInstance();
            }

            if (_httpClientSetup is not null)
            {
                _httpClientSetup.Invoke(builder);
            }
        });

        // Mock to avoid HttpClient.Dispose() not mocked exception
        Mock.Mock<IPlexApiClient>().Setup(x => x.Dispose());
    }

    private void SetDefaultBuilder(ContainerBuilder builder)
    {
        builder.Register<ILogger>((_, _) => LogFactory.Create()).SingleInstance();

        // Database context can be set up once and then retrieved by its DB name.
        builder
            .Register((_, _) => MockDatabase.GetMemoryReaparrDbContext(_databaseName))
            .As<ReaparrDbContext>()
            .InstancePerDependency();

        builder
            .Register((_, _) => MockDatabase.GetMemoryReaparrDbContext(_databaseName))
            .As<IReaparrDbContext>()
            .InstancePerDependency();

        builder
            .Register((_, _) => MockDatabase.GetMemoryAuthDbContext(_databaseName))
            .As<AuthDbContext>()
            .InstancePerDependency();

        builder
            .Register((_, _) => MockDatabase.GetMemoryAuthDbContext(_databaseName))
            .As<IAuthDbContext>()
            .InstancePerDependency();

        builder
            .Register(
                (_, _) =>
                {
                    var factoryMock = new Mock<IReaparrDbContextFactory>(MockBehavior.Strict);
                    factoryMock
                        .Setup(x => x.Create())
                        .Returns(() => MockDatabase.GetMemoryReaparrDbContext(_databaseName));
                    factoryMock
                        .Setup(x => x.CreateAsync())
                        .Returns(() =>
                            Task.FromResult<IReaparrDbContext>(MockDatabase.GetMemoryReaparrDbContext(_databaseName))
                        );
                    return factoryMock.Object;
                }
            )
            .As<IReaparrDbContextFactory>()
            .InstancePerDependency();

        builder
            .Register(
                (_, _) =>
                {
                    var factoryMock = new Mock<IAuthDbContextFactory>(MockBehavior.Strict);
                    factoryMock
                        .Setup(x => x.Create())
                        .Returns(() => MockDatabase.GetMemoryAuthDbContext(_databaseName));
                    factoryMock
                        .Setup(x => x.CreateAsync())
                        .Returns(() =>
                            Task.FromResult<IAuthDbContext>(MockDatabase.GetMemoryAuthDbContext(_databaseName))
                        );
                    return factoryMock.Object;
                }
            )
            .As<IAuthDbContextFactory>()
            .InstancePerDependency();

        builder
            .Register(_ =>
            {
                var appBuildInfoMock = new Mock<IAppBuildInfo>(MockBehavior.Loose);
                appBuildInfoMock.SetupGet(x => x.RuntimeMode).Returns("docker");
                appBuildInfoMock.SetupGet(x => x.RuntimeIdentifier).Returns("linux-x64");
                appBuildInfoMock.SetupGet(x => x.Version).Returns("0.0.0");
                appBuildInfoMock.SetupGet(x => x.InformationalVersion).Returns("0.0.0");
                appBuildInfoMock.SetupGet(x => x.CurrentOS).Returns(OperatingSystemPlatform.Linux);
                appBuildInfoMock.SetupGet(x => x.IsWindows).Returns(false);
                appBuildInfoMock.SetupGet(x => x.IsDesktopMode).Returns(false);
                appBuildInfoMock.SetupGet(x => x.IsDockerMode).Returns(true);
                appBuildInfoMock.SetupGet(x => x.IsDevRelease).Returns(false);
                return appBuildInfoMock.Object;
            })
            .As<IAppBuildInfo>()
            .SingleInstance();

        builder.Register(ctx => new PathProvider(ctx.Resolve<IAppBuildInfo>())).As<IPathProvider>().SingleInstance();
    }

    protected void SetupHttpClient(Action<Mock<HttpMessageHandler>>? action = null)
    {
        _httpClientSetup = builder =>
        {
            builder
                .Register(_ =>
                {
                    action?.Invoke(HttpHandlerMock);

                    return new HttpClient(HttpHandlerMock.Object);
                })
                .As<HttpClient>()
                .SingleInstance();
        };

        Build();
    }

    private void SetDefaultFileSystemDirectories()
    {
        var pathProvider = Mock.Container.Resolve<IPathProvider>();

        _fileSystem.AddDrive(
            "/",
            new MockDriveData
            {
                IsReady = true,
                DriveType = DriveType.Fixed,
                AvailableFreeSpace = DefaultAvailableSpace,
            }
        );
        _fileSystem.AddDirectory(pathProvider.ConfigDirectory);
        _fileSystem.AddDirectory(pathProvider.DefaultDownloadsDestinationFolder);
        _fileSystem.AddDirectory(pathProvider.DefaultMovieDestinationFolder);
        _fileSystem.AddDirectory(pathProvider.DefaultTvShowsDestinationFolder);
        _fileSystem.AddDirectory(pathProvider.DefaultMusicDestinationFolder);
        _fileSystem.AddDirectory(pathProvider.DefaultPhotosDestinationFolder);
        _fileSystem.AddDirectory(pathProvider.DefaultOtherDestinationFolder);
        _fileSystem.AddDirectory(pathProvider.DefaultGamesDestinationFolder);
    }

    protected void SetupFileSystem(Action<MockFileSystem>? action = default)
    {
        _fileSystemSetup = builder =>
        {
            if (action is not null)
            {
                action.Invoke(_fileSystem);
            }

            builder.Register<MockFileSystem>(_ => _fileSystem).As<IFileSystem>().SingleInstance();
        };

        Build();
    }

    protected void SetupFileSystem(Action<MockFileSystem, IReaparrDbContext> action)
    {
        _fileSystemSetup = builder =>
        {
            builder
                .Register<MockFileSystem>(ctx =>
                {
                    DataBaseSetupGuard();

                    var dbContext = ctx.Resolve<IReaparrDbContext>();

                    action.Invoke(_fileSystem, dbContext);

                    return _fileSystem;
                })
                .As<IFileSystem>()
                .SingleInstance();
        };

        Build();
    }
}
