using System.IO.Abstractions;
using System.Reflection;
using Autofac;
using Autofac.Features.Variance;
using FastEndpoints;
using FluentValidation;
using Reaparr.Domain;
using Module = Autofac.Module;

namespace Reaparr.Build;

public sealed class BuildModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterSource(new ContravariantRegistrationSource());

        // Core build services
        builder.RegisterType<FileSystem>().As<IFileSystem>().SingleInstance();
        builder.Register(ctx => BuildPaths.FromCurrentDirectory(ctx.Resolve<IFileSystem>())).SingleInstance();
        builder.RegisterType<FileSystemTasks>().SingleInstance();
        builder.RegisterType<DesktopCommandRunner>().As<IDesktopCommandRunner>().SingleInstance();

        var assembly = Assembly.GetExecutingAssembly();

        builder.RegisterAssemblyTypes(assembly).AsClosedTypesOf(typeof(ICommandHandler<,>)).InstancePerDependency();

        builder.RegisterAssemblyTypes(assembly).AsClosedTypesOf(typeof(IValidator<>)).InstancePerDependency();

        builder.RegisterType<BuildCommandExecutor>().As<ICommandExecutor>().InstancePerLifetimeScope();
        builder.RegisterType<EventPublisher>().As<IEventPublisher>().InstancePerLifetimeScope();

        builder.RegisterType<DesktopRunCommand>().AsSelf().InstancePerDependency();
        builder.RegisterType<DesktopPublishCommand>().AsSelf().InstancePerDependency();
        builder.RegisterType<DesktopPackageCommand>().AsSelf().InstancePerDependency();
        builder.RegisterType<DesktopCiPackageCommand>().AsSelf().InstancePerDependency();

        builder.Register(_ => Log.Logger).As<ILogger>().SingleInstance();
    }
}
