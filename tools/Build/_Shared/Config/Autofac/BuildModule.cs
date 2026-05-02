using Autofac;
using Reaparr.Domain;
using Module = Autofac.Module;

namespace Reaparr.Build;

public sealed class BuildModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Register the command executor and event publisher
        builder.RegisterType<CommandExecutor>().As<ICommandExecutor>().InstancePerLifetimeScope();
        builder.RegisterType<EventPublisher>().As<IEventPublisher>().InstancePerLifetimeScope();
    }
}
