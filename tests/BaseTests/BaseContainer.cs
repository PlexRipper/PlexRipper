using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Environment;
using FluentResultExtensions.lib;
using FluentResults;
using Logging;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using PlexRipper.Application.Common;
using PlexRipper.BaseTests.Config;
using PlexRipper.Data;
using PlexRipper.Domain;
using PlexRipper.DownloadManager.Download;
using PlexRipper.WebAPI;
using PlexRipper.WebAPI.Config;

namespace PlexRipper.BaseTests
{
    public class BaseContainer
    {
        private PlexAccount _plexAccountMain;

        private PlexAccount _plexAccountSecond;

        protected IContainer AutofacContainer { get; }

        public PlexAccount PlexAccountMain => _plexAccountMain;

        public PlexAccount PlexAccountSecond => _plexAccountSecond;

        /// <summary>
        /// Creates a Autofac container and sets up a test database.
        /// </summary>
        public BaseContainer(bool setToDiskTestDatabase = false)
        {
            EnvironmentExtensions.SetIntegrationTestMode();
            EnvironmentExtensions.SetResetDatabase();
            EnvironmentExtensions.SetInMemoryDatabase(!setToDiskTestDatabase);

            SetupSecrets();

            var builder = new ContainerBuilder();
            ContainerConfig.ConfigureContainer(builder);

            // Setup Test dependancies
            builder.RegisterModule<BaseTestsAutofacModule>();

            // re-create the startup service collection and add to builder.
            var services = new ServiceCollection();
            var startUp = new Startup(GetWebHostEnvironment());
            startUp.ConfigureServices(services);
            builder.Populate(services);

            // SignalR requires the default ILogger
            builder.RegisterInstance(new LoggerFactory()).As<ILoggerFactory>();

            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();

            AutofacContainer = builder.Build();
            PlexRipperDbContext.SetupAsync();
        }

        public async Task<PlexAccount> SetupTestAccount()
        {
            var result = await GetPlexAccountService.CreatePlexAccountAsync(_plexAccountSecond);
            if (result.IsFailed)
            {
                Log.Error("Failed to create test account for integration test!");
                return new PlexAccount();
            }

            return result.Value;
        }

        private void SetupSecrets()
        {
            var credentials = Secrets.GetCredentials().Credentials;
            if (credentials[0] is not null)
            {
                _plexAccountMain = new PlexAccount(credentials[0].Username, credentials[0].Password)
                {
                    PlexId = 100,
                    Uuid = "ABCDEFG!@#$%^",
                    Title = "Main Test Account",
                    DisplayName = "Main Test Account",
                };
            }

            if (credentials[1] is not null)
            {
                _plexAccountSecond = new PlexAccount(credentials[1].Username, credentials[1].Password)
                {
                    PlexId = 200,
                    Uuid = "ABCDEFG!@#$%^",
                    Title = "Second Test Account",
                    DisplayName = "Second Test Account",
                };
            }
        }

        public static IWebHostEnvironment GetWebHostEnvironment()
        {
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            return mockEnvironment.Object;
        }

        public IPlexAccountService GetPlexAccountService => AutofacContainer.Resolve<IPlexAccountService>();

        public IPlexServerService GetPlexServerService => AutofacContainer.Resolve<IPlexServerService>();

        public IPlexDownloadService GetPlexDownloadService => AutofacContainer.Resolve<IPlexDownloadService>();

        public IPlexLibraryService GetPlexLibraryService => AutofacContainer.Resolve<IPlexLibraryService>();

        public IPlexApiService GetPlexApiService => AutofacContainer.Resolve<IPlexApiService>();

        public IDownloadManager GetDownloadManager => AutofacContainer.Resolve<IDownloadManager>();

        public IDownloadQueue GetDownloadQueue => AutofacContainer.Resolve<IDownloadQueue>();

        public IFolderPathService GetFolderPathService => AutofacContainer.Resolve<IFolderPathService>();

        public IPlexDownloadTaskFactory GetPlexDownloadTaskFactory => AutofacContainer.Resolve<IPlexDownloadTaskFactory>();

        public IPlexRipperHttpClient GetPlexRipperHttpClient => AutofacContainer.Resolve<IPlexRipperHttpClient>();

        public Func<DownloadTask, PlexDownloadClient> GetPlexDownloadClientFactory =>
            AutofacContainer.Resolve<Func<DownloadTask, PlexDownloadClient>>();

        public PlexRipperDbContext PlexRipperDbContext => AutofacContainer.Resolve<PlexRipperDbContext>();

        public IMediator Mediator => AutofacContainer.Resolve<IMediator>();

        public IMockServer MockServer => AutofacContainer.Resolve<IMockServer>();

        public IFakeData FakeData => AutofacContainer.Resolve<IFakeData>();

        public IFileSystem FileSystem => AutofacContainer.Resolve<IFileSystem>();

        public IPathSystem PathSystem => AutofacContainer.Resolve<IPathSystem>();
    }
}