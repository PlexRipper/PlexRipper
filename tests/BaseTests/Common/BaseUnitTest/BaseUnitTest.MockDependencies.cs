using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Autofac;
using ByteSizeLib;
using Reaparr.Data;
using Reaparr.Data.Contracts;
using Reaparr.Environment;
using Reaparr.Identity;
using Reaparr.Identity.Contracts;
using Reaparr.Logging;
using Reaparr.PlexApi.Contracts;
using Serilog;

namespace Reaparr.BaseTests;

public partial class BaseUnitTest
{
    private Action<ContainerBuilder>? _fileSystemSetup;
    private Action<ContainerBuilder>? _httpClientSetup;
    private readonly MockFileSystem _fileSystem = new();
    protected AutoMock mock { get; set; }

    protected long DefaultAvailableSpace = (long)ByteSize.FromGigaBytes(1000).Bytes;

    private void Build()
    {
        mock = AutoMock.GetStrict(builder =>
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
        mock.Mock<IPlexApiClient>().Setup(x => x.Dispose());
    }

    private void SetDefaultBuilder(ContainerBuilder builder)
    {
        builder
            .Register<ILogger>(
                (_, _) =>
                {
                    var logConfig = new TestLogConfig(_output);
                    LogManager.SetupLogging(_logEventLevel); //TODO might need to be removed if LogManager
                    return logConfig.GetLogger(_logEventLevel);
                }
            )
            .SingleInstance();

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
            new MockDriveData
            {
                IsReady = true,
                DriveType = DriveType.Fixed,
                AvailableFreeSpace = DefaultAvailableSpace,
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
