using System;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using FluentValidation;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using PlexRipper.Application.Common;
using PlexRipper.Domain;

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

            // Setup Database
            var dbContextOptions = PlexRipperDbContext.GetConfig().Options;

            var testMode = Environment.GetEnvironmentVariable("IntegrationTestMode");
            if (testMode != null && testMode == "true")
            {
                // Register Entity Framework Database in TestMode for IntegrationTests
                dbContextOptions = PlexRipperDbContext.GetTestConfig().Options;
            }

            builder.RegisterType<PlexRipperDbContext>()
                .WithParameter("options", dbContextOptions)
                .As<IPlexRipperDbContext>()
                .InstancePerDependency();

            builder.RegisterType<PlexRipperDbContext>()
                .WithParameter("options", dbContextOptions)
                .InstancePerDependency(); // TODO this might need to be InstancePerLifetime
        }
    }
}