using System;
using System.Reflection;
using Autofac;
using FluentValidation;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;

namespace PlexRipper.Data.Config
{
    public class DataModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // MediatR
            builder.AddMediatR(assembly);

            // Register the Command's Validators (Validators based on FluentValidation library)
            builder.RegisterAssemblyTypes(assembly)
                .Where(t => t.IsClosedTypeOf(typeof(IValidator<>)))
                .AsImplementedInterfaces();

            // Register all the Command classes (they implement IRequestHandler) in assembly holding the Commands
            builder.RegisterAssemblyTypes(assembly)
                .AsClosedTypesOf(typeof(IRequestHandler<,>));

            builder.RegisterType<PlexRipperDbContext>()
                .InstancePerDependency(); // TODO this might need to be InstancePerLifetime
        }
    }
}