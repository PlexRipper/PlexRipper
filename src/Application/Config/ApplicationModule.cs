using Autofac;
using FluentValidation;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using PlexRipper.Application.Common.Interfaces.Application;
using PlexRipper.Application.Common.TestClass;
using PlexRipper.Domain.Behavior.Pipelines;
using System.Reflection;

namespace PlexRipper.Application.Config
{
    public class ApplicationModule : Autofac.Module
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
            builder.RegisterGeneric(typeof(ValidationPipeline<,>)).As(typeof(IPipelineBehavior<,>));

            // register all I*Services
            builder.RegisterAssemblyTypes(assembly)
                .Where(t => t.Name.EndsWith("Service"))
                .AsImplementedInterfaces()
                .InstancePerDependency();

            // Used for testing purposes
            builder.RegisterType<TestClass>().As<ITestClass>().SingleInstance();
        }
    }
}
