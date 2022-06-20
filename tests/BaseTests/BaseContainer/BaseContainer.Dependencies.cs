#region

using System;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Environment;
using JetBrains.Annotations;
using Logging;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PlexRipper.Application;
using PlexRipper.Data;
using PlexRipper.Domain;
using PlexRipper.DownloadManager;
using PlexRipper.WebAPI;

#endregion

namespace PlexRipper.BaseTests
{
    public partial class BaseContainer : IDisposable
    {
        #region Fields

        private readonly WebApplicationFactory<Startup> _factory;

        private readonly IServiceScope _serviceScope;

        private readonly IServiceProvider _services;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a Autofac container and sets up a test database.
        /// </summary>
        private BaseContainer(string memoryDbName, [CanBeNull] Action<UnitTestDataConfig> options = null)
        {
            _factory = new PlexRipperWebApplicationFactory<Startup>(memoryDbName, options);
            ApiClient = _factory.CreateClient();

            // Create a separate scope as not to interfere with tests running in parallel
            _services = _factory.Services;
            _serviceScope = _factory.Services.CreateScope();
        }

        public static async Task<BaseContainer> Create([CanBeNull] Action<UnitTestDataConfig> options = null)
        {
            var config = UnitTestDataConfig.FromOptions(options);
            var memoryDbName = MockDatabase.GetMemoryDatabaseName();

            Log.Information($"Setting up BaseContainer with database: {memoryDbName}");

            EnvironmentExtensions.SetIntegrationTestMode(true);

            // Database context can be setup once and then retrieved by its DB name.
            await MockDatabase.GetMemoryDbContext(memoryDbName).Setup(options);
            var container = new BaseContainer(memoryDbName, options);

            if (config.DownloadSpeedLimit > 0)
            {
                await container.SetDownloadSpeedLimit(options);
            }

            return container;
        }

        #endregion

        #region Properties

        public System.Net.Http.HttpClient ApiClient { get; }

        #region Autofac Resolve

        public IFileSystem FileSystem => Resolve<IFileSystem>();

        public IDownloadCommands GetDownloadCommands => Resolve<IDownloadCommands>();

        public IDownloadQueue GetDownloadQueue => Resolve<IDownloadQueue>();

        public IDownloadTaskFactory GetDownloadTaskFactory => Resolve<IDownloadTaskFactory>();

        public IDownloadTaskValidator GetDownloadTaskValidator => Resolve<IDownloadTaskValidator>();

        public IDownloadTracker GetDownloadTracker => Resolve<IDownloadTracker>();

        public IFolderPathService GetFolderPathService => Resolve<IFolderPathService>();

        public IPlexAccountService GetPlexAccountService => Resolve<IPlexAccountService>();

        public IPlexApiService GetPlexApiService => Resolve<IPlexApiService>();

        public IPlexDownloadService GetPlexDownloadService => Resolve<IPlexDownloadService>();

        public IPlexLibraryService GetPlexLibraryService => Resolve<IPlexLibraryService>();

        public IPlexRipperHttpClient GetPlexRipperHttpClient => Resolve<IPlexRipperHttpClient>();

        public IPlexServerService GetPlexServerService => Resolve<IPlexServerService>();

        public IMediator Mediator => Resolve<IMediator>();

        public IPathProvider PathProvider => Resolve<IPathProvider>();

        public ITestStreamTracker TestStreamTracker => Resolve<ITestStreamTracker>();

        public ITestApplicationTracker TestApplicationTracker => Resolve<ITestApplicationTracker>();

        public PlexRipperDbContext PlexRipperDbContext => Resolve<PlexRipperDbContext>();

        public ISchedulerService SchedulerService => Resolve<ISchedulerService>();

        public IFileMerger FileMerger => Resolve<IFileMerger>();

        public IMapper Mapper => Resolve<IMapper>();

        public IHostLifetime Boot => Resolve<IHostLifetime>();

        #region Settings

        public IServerSettingsModule GetServerSettings => Resolve<IServerSettingsModule>();

        public IConfigManager ConfigManager => Resolve<IConfigManager>();

        #endregion

        #endregion

        #endregion

        #region Public Methods

        private T Resolve<T>()
        {
            return _services.GetRequiredService<T>();
        }

        #endregion

        public void Dispose()
        {
            PlexRipperDbContext.Database.EnsureDeleted();
            _services.GetAutofacRoot().Dispose();
            _factory?.Dispose();
            ApiClient?.Dispose();
        }
    }
}