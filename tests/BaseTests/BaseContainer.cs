using System.Text.Json.Serialization;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutofacSerilogIntegration;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using PlexRipper.Application.Common;
using PlexRipper.WebAPI;
using PlexRipper.WebAPI.Config;

namespace PlexRipper.BaseTests
{
    public class BaseContainer
    {
        protected IContainer AutofacContainer { get; }

        public BaseContainer()
        {
            var builder = new ContainerBuilder();
            ContainerConfig.ConfigureContainer(builder);

            // re-create the startup service collection and add to builder.
            var services = new ServiceCollection();
            var startUp = new Startup();
            startUp.ConfigureServices(services);
            builder.Populate(services);

            builder.RegisterLogger(autowireProperties: true);
            AutofacContainer = builder.Build();
        }


        public IPlexAccountService GetPlexAccountService => AutofacContainer.Resolve<IPlexAccountService>();

        public IPlexServerService GetPlexServerService => AutofacContainer.Resolve<IPlexServerService>();

        public IPlexDownloadService GetPlexDownloadService => AutofacContainer.Resolve<IPlexDownloadService>();

        public IPlexLibraryService GetPlexLibraryService => AutofacContainer.Resolve<IPlexLibraryService>();

        public IPlexApiService GetPlexApiService => AutofacContainer.Resolve<IPlexApiService>();

        public IDownloadManager GetDownloadManager => AutofacContainer.Resolve<IDownloadManager>();
    }
}