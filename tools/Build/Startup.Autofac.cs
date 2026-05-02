using Autofac;
using FastEndpoints;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;

namespace Reaparr.Build;

public static partial class Startup
{
    public static AutofacTypeRegistrar CreateContainer(ILogger logger)
    {
        var services = new ServiceCollection();

        services.AddSerilog(logger);

        services.AddFastEndpoints(options =>
        {
            options.DisableAutoDiscovery = true;
            options.Assemblies = [Assembly.GetExecutingAssembly()];
        });

        return new AutofacTypeRegistrar(services, builder => builder.RegisterModule<BuildModule>());
    }
}
