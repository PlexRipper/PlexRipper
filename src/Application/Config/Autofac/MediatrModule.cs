using System.Reflection;
using Autofac;
using FluentValidation;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using MediatR.Pipeline;
using Module = Autofac.Module;

namespace PlexRipper.Application;

public class MediatrModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // MediatR
        var configuration = MediatRConfigurationBuilder
            .Create(assembly)
            .WithAllOpenGenericHandlerTypesRegistered()
            .Build();
        builder.RegisterMediatR(configuration);

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

        // Register Behavior Pipeline
        builder.RegisterGeneric(typeof(ValidationPipeline<,>)).As(typeof(IPipelineBehavior<,>));
        builder.RegisterGeneric(typeof(ExceptionLoggingHandler<,,>)).As(typeof(IRequestExceptionHandler<,,>));
    }
}
