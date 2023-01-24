using Autofac;
using Logging.Interface;
using Serilog;

namespace Logging;

/// <summary>
/// Add the default test mock modules here which can later be overridden
/// </summary>
public class LogModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register<ILogger>((c, p) => LogConfig.GetTestLogger()).SingleInstance();
        builder.RegisterType<Log2.Log>().As<ILog>().SingleInstance();
    }
}