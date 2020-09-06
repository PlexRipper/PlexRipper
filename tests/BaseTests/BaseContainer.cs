using Autofac;
using AutofacSerilogIntegration;
using PlexRipper.Application.Common;
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