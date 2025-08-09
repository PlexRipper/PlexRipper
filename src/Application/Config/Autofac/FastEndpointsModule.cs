using System.Reflection;
using Autofac;
using FluentValidation;
using Module = Autofac.Module;

namespace PlexRipper.Application;

public class FastEndpointsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<CommandExecutor>().As<ICommandExecutor>().InstancePerLifetimeScope();

        builder.RegisterType<EventPublisher>().As<IEventPublisher>().SingleInstance();

        var assembly = Assembly.GetExecutingAssembly();

        // register all I*Commands
        builder
            .RegisterAssemblyTypes(assembly)
            .Where(t => t.Name.EndsWith("Command"))
            .AsImplementedInterfaces()
            .SingleInstance();

        // register all FluentValidators
        builder
            .RegisterAssemblyTypes(assembly)
            .Where(t => t.IsClosedTypeOf(typeof(IValidator<>)))
            .AsImplementedInterfaces();
    }
}
