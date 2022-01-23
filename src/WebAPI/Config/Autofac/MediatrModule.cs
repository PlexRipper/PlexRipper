using Autofac;
using FluentValidation;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using PlexRipper.Data;
using PlexRipper.Domain.Behavior.Pipelines;
using Module = Autofac.Module;

namespace PlexRipper.WebAPI.Config
{
    public class MediatrModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assembly = typeof(PlexRipperDbContext).Assembly;

            // MediatR
            builder.RegisterMediatR(assembly);

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
}