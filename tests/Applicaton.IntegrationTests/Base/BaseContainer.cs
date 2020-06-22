using Autofac;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.WebAPI.Config;
using Serilog;

namespace PlexRipper.Application.IntegrationTests.Base
{
    public class BaseContainer
    {
        protected IContainer AutofacContainer { get; }

        public BaseContainer()
        {
            var builder = new ContainerBuilder();
            ContainerConfig.ConfigureContainer(builder);
            builder.Register<ILogger>((c, p) => BaseDependanciesTest.GetLoggerConfig()).SingleInstance();
            AutofacContainer = builder.Build();
        }


        public IAccountService GetAccountService => AutofacContainer.Resolve<IAccountService>();
        public IPlexServerService GetPlexServerService => AutofacContainer.Resolve<IPlexServerService>();
        public IPlexService GetPlexService => AutofacContainer.Resolve<IPlexService>();
        public IPlexDownloadService GetPlexDownloadService => AutofacContainer.Resolve<IPlexDownloadService>();
        public IPlexLibraryService GetPlexLibraryService => AutofacContainer.Resolve<IPlexLibraryService>();

    }
}
