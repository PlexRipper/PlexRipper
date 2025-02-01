using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Autofac;
using ByteSizeLib;
using Data.Contracts;
using Environment;
using Logging.Interface;
using PlexApi.Contracts;
using PlexRipper.Data;
using PlexRipper.Identity;
using PlexRipper.Identity.Contracts;
using Serilog;
using Log = Logging.Log;

namespace PlexRipper.BaseTests;

public partial class BaseUnitTest : IDisposable
{
    private Action<ContainerBuilder>? _fileSystemSetup;
    private Action<ContainerBuilder>? _httpClientSetup;
    private readonly MockFileSystem _fileSystem = new();
    protected AutoMock mock { get; set; }

    private void Build()
    {
        mock = AutoMock.GetStrict(builder =>
        {
            SetDefaultBuilder(builder);

            if (_fileSystemSetup is not null)
            {
                SetDefaultFileSystemDirectories();
                _fileSystemSetup.Invoke(builder);
            }

            if (_httpClientSetup is not null)
            {
                _httpClientSetup.Invoke(builder);
            }
        });

        // Mock to avoid HttpClient.Dispose() not mocked exception
        mock.Mock<IPlexApiClient>().Setup(x => x.Dispose());
    }

    private void SetDefaultBuilder(ContainerBuilder builder)
    {
        builder
            .Register<ILogger>(
                (_, _) =>
                {
                    LogManager.SetupLogging(_logEventLevel);
                    LogConfig.SetTestOutputHelper(_output);
                    return LogConfig.GetLogger();
                }
            )
            .SingleInstance();

        // Database context can be setup once and then retrieved by its DB name.
        builder
            .Register((_, _) => MockDatabase.GetMemoryPlexRipperDbContext(_databaseName))
            .As<PlexRipperDbContext>()
            .InstancePerDependency();

        builder
            .Register((_, _) => MockDatabase.GetMemoryPlexRipperDbContext(_databaseName))
            .As<IPlexRipperDbContext>()
            .InstancePerDependency();

        builder
            .Register((_, _) => MockDatabase.GetMemoryAuthDbContext(_databaseName))
            .As<AuthDbContext>()
            .InstancePerDependency();

        builder
            .Register((_, _) => MockDatabase.GetMemoryAuthDbContext(_databaseName))
            .As<IAuthDbContext>()
            .InstancePerDependency();

        builder.RegisterType<Log>().As<ILog>().SingleInstance();
        builder.RegisterGeneric(typeof(Log<>)).As(typeof(ILog<>)).InstancePerDependency();

        builder.Register(ctx => ctx.Resolve<IFileSystem>().Path).As<IPath>().SingleInstance();
        builder.Register(ctx => ctx.Resolve<IFileSystem>().File).As<IFile>().SingleInstance();
        builder.Register(ctx => ctx.Resolve<IFileSystem>().Directory).As<IDirectory>().SingleInstance();
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
        _fileSystem.AddDrive(
            "/",
            new MockDriveData()
            {
                IsReady = true,
                DriveType = DriveType.Fixed,
                AvailableFreeSpace = (long)ByteSize.FromGigaBytes(1000).Bytes,
            }
        );
        _fileSystem.AddDirectory(PathProvider.ConfigDirectory);
        _fileSystem.AddDirectory(PathProvider.DefaultDownloadsDestinationFolder);
        _fileSystem.AddDirectory(PathProvider.DefaultMovieDestinationFolder);
        _fileSystem.AddDirectory(PathProvider.DefaultTvShowsDestinationFolder);
        _fileSystem.AddDirectory(PathProvider.DefaultMusicDestinationFolder);
        _fileSystem.AddDirectory(PathProvider.DefaultPhotosDestinationFolder);
        _fileSystem.AddDirectory(PathProvider.DefaultOtherDestinationFolder);
        _fileSystem.AddDirectory(PathProvider.DefaultGamesDestinationFolder);
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

    protected void SetupFileSystem(Action<MockFileSystem, IPlexRipperDbContext> action)
    {
        _fileSystemSetup = builder =>
        {
            builder
                .Register<MockFileSystem>(ctx =>
                {
                    DataBaseSetupGuard();

                    var dbContext = ctx.Resolve<IPlexRipperDbContext>();

                    action.Invoke(_fileSystem, dbContext);

                    return _fileSystem;
                })
                .As<IFileSystem>()
                .SingleInstance();
        };

        Build();
    }
}
