using System.Reflection;
using Autofac;
using FluentValidation;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using PlexRipper.Application.Common;
using Module = Autofac.Module;

namespace PlexRipper.Data.Config
{
    public class DataModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // MediatR
            builder.RegisterMediatR(assembly);

            // Register the Command's Validators (Validators based on FluentValidation library)
            builder.RegisterAssemblyTypes(assembly)
                .Where(t => t.IsClosedTypeOf(typeof(IValidator<>)))
                .AsImplementedInterfaces();

            // Register all the Command classes (they implement IRequestHandler) in assembly holding the Commands
            builder.RegisterAssemblyTypes(assembly)
                .AsClosedTypesOf(typeof(IRequestHandler<,>));

            // We get concurrency issues s if we have .InstancePerLifetimeScope();
            builder.RegisterType<PlexRipperDbContext>()
                .InstancePerDependency();

            builder.RegisterType<PlexRipperDatabaseService>().As<IPlexRipperDatabaseService>()
                .InstancePerDependency();
        }
    }
}