using Autofac;
using AutoMapper;
using BackgroundServices;
using PlexRipper.Application;
using PlexRipper.Data;
using PlexRipper.FileSystem.Config;
using PlexRipper.PlexApi;
using PlexRipper.Settings.Config;

namespace PlexRipper.WebAPI;

public static class ContainerConfig
{
    /// <summary>
    /// Autofac container builder, Serilog registration is left out due to being context dependent.
    /// Integration tests have a different configuration than the application.
    /// </summary>
    /// <param name="builder">The builder through which components can be registered.</param>
    public static void ConfigureContainer(ContainerBuilder builder)
    {
        // Application
        builder.RegisterModule<ApplicationModule>();
        builder.RegisterModule<LogModule>();

        // Infrastructure
        builder.RegisterModule<DataModule>();
        builder.RegisterModule<FileSystemModule>();
        builder.RegisterModule<PlexApiModule>();
        builder.RegisterModule<SettingsModule>();
        builder.RegisterModule<HttpClientModule>();
        builder.RegisterModule<BackgroundServicesModule>();

        // Presentation
        builder.RegisterModule<WebApiModule>();

        // Packages
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