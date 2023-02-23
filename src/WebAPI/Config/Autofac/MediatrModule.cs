using Autofac;
using FluentValidation;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using PlexRipper.Data;
using PlexRipper.Domain.Behavior.Pipelines;

namespace PlexRipper.WebAPI;

public class MediatrModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var assembly = typeof(PlexRipperDbContext).Assembly;

        // MediatR
        var configuration = MediatRConfigurationBuilder
            .Create(assembly)
            .WithAllOpenGenericHandlerTypesRegistered()
            .WithRegistrationScope(RegistrationScope.Transient)
            .Build();
        builder.RegisterMediatR(configuration);

        // Register the Command's Validators (Validators based on FluentValidation library)
        builder.RegisterAssemblyTypes(assembly)
            .Where(t => t.IsClosedTypeOf(typeof(IValidator<>)))
            .AsImplementedInterfaces();

        // Register all the Command classes (they implement IRequestHandler) in assembly holding the Commands
        builder.RegisterAssemblyTypes(assembly)
            .AsClosedTypesOf(typeof(IRequestHandler<,>));

        // Register Behavior Pipeline
        builder.RegisterGeneric(typeof(ValidationPipeline<,>)).As(typeof(IPipelineBehavior<,>));
    }
}