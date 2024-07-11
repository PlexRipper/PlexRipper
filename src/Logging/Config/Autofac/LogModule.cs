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
        builder.Register<ILogger>((_, _) => LogConfig.GetLogger()).SingleInstance();
        builder.RegisterType<Log>().As<ILog>().SingleInstance();
        builder.RegisterGeneric(typeof(Log<>)).As(typeof(ILog<>)).InstancePerDependency();
    }
}
