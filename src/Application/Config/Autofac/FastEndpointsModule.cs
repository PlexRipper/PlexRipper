using Autofac;
using Module = Autofac.Module;

namespace PlexRipper.Application;

public class FastEndpointsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<CommandExecutor>().As<ICommandExecutor>().InstancePerLifetimeScope();

        builder.RegisterType<EventPublisher>().As<IEventPublisher>().SingleInstance();
    }
}
