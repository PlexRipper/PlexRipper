using System.Reflection;
using Autofac;
using FastEndpoints;
using Module = Autofac.Module;

namespace Reaparr.Application;

public class FastEndpointsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register the command executor and event publisher
        builder.RegisterType<CommandExecutor>().As<ICommandExecutor>().InstancePerLifetimeScope();
        builder.RegisterType<EventPublisher>().As<IEventPublisher>().SingleInstance();

        // Register all command handlers implementing ICommandHandler<TCommand, TResult>
        builder
            .RegisterAssemblyTypes(assembly)
            .AsClosedTypesOf(typeof(ICommandHandler<,>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        // Register all event handlers implementing IEventHandler<TEvent>
        builder
            .RegisterAssemblyTypes(assembly)
            .AsClosedTypesOf(typeof(IEventHandler<>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
    }
}
