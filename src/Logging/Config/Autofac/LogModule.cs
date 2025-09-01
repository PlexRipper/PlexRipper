using Autofac;
using Serilog;

namespace Reaparr.Logging;

/// <summary>
/// Add the default test mock modules here which can later be overridden
/// </summary>
public class LogModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();
    }
}
