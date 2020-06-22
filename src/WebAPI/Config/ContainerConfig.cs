using Autofac;
using AutoMapper;
using PlexRipper.Application.Config;
using PlexRipper.Application.Config.Mappings;
using PlexRipper.Domain.AutoMapper;
using PlexRipper.DownloadManager.Config;
using PlexRipper.Infrastructure.Common.Mappings;
using PlexRipper.Infrastructure.Config;
using PlexRipper.Settings.Config;

namespace PlexRipper.WebAPI.Config
{
    public static class ContainerConfig
    {
        /// <summary>
        /// Autofac container builder, Serilog registration is left out due to being context dependent. Integration tests have a different configuration than the application. 
        /// </summary>
        /// <param name="builder"></param>
        public static void ConfigureContainer(ContainerBuilder builder)
        {
            // Application
            builder.RegisterModule<ApplicationModule>();

            // Infrastructure
            builder.RegisterModule<InfrastructureModule>();
            builder.RegisterModule<SettingsModule>();
            builder.RegisterModule<DownloadManagerModule>();

            // Auto Mapper
            builder.Register(ctx =>
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile(new DomainMappingProfile());
                    cfg.AddProfile(new ApplicationMappingProfile());
                    cfg.AddProfile(new InfrastructureMappingProfile());
                    cfg.AddProfile(new WebApiMappingProfile());
                });
                config.AssertConfigurationIsValid();
                return config;
            });

            builder.Register(ctx => ctx.Resolve<MapperConfiguration>().CreateMapper()).As<IMapper>().InstancePerLifetimeScope();
        }
    }
}
