using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace Reaparr.Build;

public static partial class Startup
{
    public static AutofacTypeRegistrar CreateContainer(ILogger logger)
    {
        var services = new ServiceCollection();

        services.AddSerilog(logger);

        return new AutofacTypeRegistrar(services, builder => builder.RegisterModule<BuildModule>());
    }
}
