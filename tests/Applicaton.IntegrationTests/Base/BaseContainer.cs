using Autofac;
using AutofacSerilogIntegration;
using AutoMapper;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Mappings;
using PlexRipper.Application.Config;
using PlexRipper.Infrastructure.Common.Mappings;
using PlexRipper.Infrastructure.Config;

namespace PlexRipper.Application.IntegrationTests.Base
{
    public class BaseContainer
    {
        protected IContainer AutofacContainer { get; }

        public BaseContainer()
        {

            var builder = new ContainerBuilder();
            // Application
            builder.RegisterModule<ApplicationModule>();

            // Infrastructure
            builder.RegisterModule<InfrastructureModule>();
            builder.RegisterLogger(BaseDependanciesTest.GetLoggerConfig());

            // Auto Mapper
            builder.Register(ctx => new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ApplicationMappingProfile());
                cfg.AddProfile(new InfrastructureMappingProfile());
            }));

            builder.Register(ctx => ctx.Resolve<MapperConfiguration>().CreateMapper()).As<IMapper>().InstancePerLifetimeScope();

            AutofacContainer = builder.Build();
        }


        public IAccountService GetAccountService => AutofacContainer.Resolve<IAccountService>();
        public IPlexServerService GetPlexServerService => AutofacContainer.Resolve<IPlexServerService>();
        public IPlexService GetPlexService => AutofacContainer.Resolve<IPlexService>();

    }
}
