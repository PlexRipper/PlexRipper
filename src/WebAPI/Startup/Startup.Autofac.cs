using Autofac;
using Autofac.Extensions.DependencyInjection;
using Serilog;

namespace PlexRipper.WebAPI;

public static partial class Startup
{
    /// <summary>
    ///  This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="builder"></param>
    public static void ConfigureAutofacBuilder(this IHostBuilder builder)
    {
        // Use Autofac as the DI container
        builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        builder.ConfigureContainer<ContainerBuilder>(containerBuilder =>
        {
            _log.DebugLine("Setting up Autofac Containers");
            ContainerConfig.ConfigureContainer(containerBuilder);
        });

        // Add services to the container.
        builder.UseSerilog(new LogConfig().GetLogger());
    }
}
