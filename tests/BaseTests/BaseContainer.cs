using Autofac;
using AutofacSerilogIntegration;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.Application;
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


        public IAccountService GetAccountService => AutofacContainer.Resolve<IAccountService>();
        public IPlexServerService GetPlexServerService => AutofacContainer.Resolve<IPlexServerService>();
        public IPlexService GetPlexService => AutofacContainer.Resolve<IPlexService>();
        public IPlexDownloadService GetPlexDownloadService => AutofacContainer.Resolve<IPlexDownloadService>();
        public IPlexLibraryService GetPlexLibraryService => AutofacContainer.Resolve<IPlexLibraryService>();
        public ITestClass GetTestClass => AutofacContainer.Resolve<ITestClass>();
        public IPlexApiService GetPlexApiService => AutofacContainer.Resolve<IPlexApiService>();

    }
}
