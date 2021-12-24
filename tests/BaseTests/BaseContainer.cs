using System;
using Autofac;
using AutoMapper;
using Environment;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using PlexRipper.Application;
using PlexRipper.Data;
using PlexRipper.Domain;
using PlexRipper.DownloadManager;
using PlexRipper.WebAPI;

namespace PlexRipper.BaseTests
{
    public class BaseContainer
    {
        #region Fields

        private readonly WebApplicationFactory<Startup> _factory;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a Autofac container and sets up a test database.
        /// </summary>
        public BaseContainer(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            EnvironmentExtensions.SetIntegrationTestMode(true);
            _factory = new PlexRipperWebApplicationFactory<Startup>(config);
            ApiClient = _factory.CreateClient();
        }

        #endregion

        #region Properties

        public System.Net.Http.HttpClient ApiClient { get; }

        #region Autofac Resolve

        public IFileSystem FileSystem => Resolve<IFileSystem>();

        public IDownloadCommands GetDownloadCommands => Resolve<IDownloadCommands>();

        public IDownloadManager GetDownloadManager => Resolve<IDownloadManager>();

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

        public IMapper Mapper => Resolve<IMapper>();

        public IPathSystem PathSystem => Resolve<IPathSystem>();

        //TODO This should be deleted
        public IPlexMockServer PlexMockServer => Resolve<IPlexMockServer>();

        public PlexRipperDbContext PlexRipperDbContext => Resolve<PlexRipperDbContext>();

        #endregion

        #endregion

        #region Public Methods

        private T Resolve<T>()
        {
            return _factory.Server.Services.GetRequiredService<T>();
        }

        #endregion
    }
}