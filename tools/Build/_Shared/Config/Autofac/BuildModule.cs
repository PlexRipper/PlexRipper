using Autofac;
using Reaparr.Domain;
using System.IO.Abstractions;
using Module = Autofac.Module;

namespace Reaparr.Build;

public sealed class BuildModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Core build services
        builder.RegisterType<FileSystem>().As<IFileSystem>().SingleInstance();
        builder.Register(ctx => BuildPaths.FromCurrentDirectory(ctx.Resolve<IFileSystem>())).SingleInstance();
        builder.RegisterType<FileSystemTasks>().SingleInstance();

        // Command infrastructure
        builder.RegisterType<CommandExecutor>().As<ICommandExecutor>().InstancePerLifetimeScope();
        builder.RegisterType<EventPublisher>().As<IEventPublisher>().InstancePerLifetimeScope();

        // Desktop command dependencies
        builder.RegisterType<DesktopRunCommand>().AsSelf().InstancePerDependency();
        builder.RegisterType<DesktopPublishCommand>().AsSelf().InstancePerDependency();
        builder.RegisterType<DesktopPackageCommand>().AsSelf().InstancePerDependency();
        builder.RegisterType<DesktopCiPackageCommand>().AsSelf().InstancePerDependency();
    }
}
