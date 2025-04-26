using Autofac;

namespace PlexRipper.Application;

public class FastEndpointsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<CommandDispatch>().As<ICommandDispatch>().SingleInstance();
    }
}
