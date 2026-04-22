using Autofac.Extensions.DependencyInjection;

namespace Reaparr.AppHost;

public static partial class Startup
{
    /// <summary>
    ///  This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="logBuffer">Pre-built log buffer instance to share with the DI container.</param>
    public static void ConfigureAutofacBuilder(this IHostBuilder builder, ILogBufferService logBuffer)
    {
        // Use Autofac as the DI container
        builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        builder.ConfigureContainer<ContainerBuilder>(containerBuilder =>
        {
            _log.Here().Debug("Setting up Autofac Containers");

            containerBuilder.RegisterInstance(logBuffer).As<ILogBufferService>().SingleInstance();

            ContainerConfig.ConfigureContainer(containerBuilder);
        });

        // Add services to the container.
        builder.UseSerilog(LogFactory.Create());
    }
}
