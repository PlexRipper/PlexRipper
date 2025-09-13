using Autofac;
using Reaparr.Application;
using Reaparr.Data;
using Reaparr.FileSystem.Config;
using Reaparr.Identity;
using Reaparr.PlexApi;
using Reaparr.Settings.Config;

namespace Reaparr.AppHost;

/// <summary>
///  The ContainerConfig class contains all the Autofac modules.
/// </summary>
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
        builder.RegisterModule<IdentityModule>();
        builder.RegisterModule<FileSystemModule>();
        builder.RegisterModule<PlexApiModule>();
        builder.RegisterModule<SettingsModule>();

        // Presentation
        builder.RegisterModule<AppHostModule>();

        // Packages
        builder.RegisterModule<QuartzModule>();
        builder.RegisterModule<FastEndpointsModule>();
    }
}
