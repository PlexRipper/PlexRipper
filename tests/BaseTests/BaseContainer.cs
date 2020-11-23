using System;
using Autofac;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Data;
using PlexRipper.WebAPI.Config;

namespace PlexRipper.BaseTests
{
    public class BaseContainer
    {
        protected IContainer AutofacContainer { get; }

        public BaseContainer()
        {
            Environment.SetEnvironmentVariable("IntegrationTestMode", "true");
            Environment.SetEnvironmentVariable("ResetDB", "true");

            var builder = new ContainerBuilder();
            ContainerConfig.ConfigureContainer(builder);

            // re-create the startup service collection and add to builder.
            // var services = new ServiceCollection();
            // var startUp = new Startup();
            // startUp.ConfigureServices(services);
            // builder.Populate(services);

            // Database setup
            PlexRipperDBSetup.Setup();

            AutofacContainer = builder.Build();
        }


        public IPlexAccountService GetPlexAccountService => AutofacContainer.Resolve<IPlexAccountService>();

        public IPlexServerService GetPlexServerService => AutofacContainer.Resolve<IPlexServerService>();

        public IPlexDownloadService GetPlexDownloadService => AutofacContainer.Resolve<IPlexDownloadService>();

        public IPlexLibraryService GetPlexLibraryService => AutofacContainer.Resolve<IPlexLibraryService>();

        public IPlexApiService GetPlexApiService => AutofacContainer.Resolve<IPlexApiService>();

        public IDownloadManager GetDownloadManager => AutofacContainer.Resolve<IDownloadManager>();

        public PlexRipperDbContext PlexRipperDbContext => AutofacContainer.Resolve<PlexRipperDbContext>();

        public IMediator Mediator => AutofacContainer.Resolve<IMediator>();
    }
}