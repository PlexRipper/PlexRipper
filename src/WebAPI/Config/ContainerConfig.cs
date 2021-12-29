using Autofac;
using AutoMapper;
using PlexRipper.Application;
using PlexRipper.Data;
using PlexRipper.DownloadManager;
using PlexRipper.FileSystem.Config;
using PlexRipper.HttpClient.Config;
using PlexRipper.PlexApi.Config;
using PlexRipper.Settings.Config;

namespace PlexRipper.WebAPI.Config
{
    public static class ContainerConfig
    {
        /// <summary>
        /// Autofac container builder, Serilog registration is left out due to being context dependent. Integration tests have a different configuration than the application.
        /// </summary>
        /// <param name="builder">The builder through which components can be registered.</param>
        public static void ConfigureContainer(ContainerBuilder builder)
        {
            // Application
            builder.RegisterModule<ApplicationModule>();

            // Infrastructure
            builder.RegisterModule<DataModule>();
            builder.RegisterModule<DownloadManagerModule>();
            builder.RegisterModule<FileSystemModule>();
            builder.RegisterModule<PlexApiModule>();
            builder.RegisterModule<SettingsModule>();
            builder.RegisterModule<HttpClientModule>();

            // Presentation
            builder.RegisterModule<WebApiModule>();

            // Packages
            builder.RegisterModule<QuartzModule>();
            builder.RegisterModule<MediatrModule>();

            // Auto Mapper
            builder.Register(_ =>
            {
                var config = MapperSetup.Configuration;
                config.AssertConfigurationIsValid();
                return config;
            });

            builder.Register(ctx => ctx.Resolve<MapperConfiguration>().CreateMapper()).As<IMapper>().InstancePerLifetimeScope();
        }
    }
}