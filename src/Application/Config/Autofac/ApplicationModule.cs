using System.Reflection;
using Application.Contracts;
using Autofac;
using FileSystem.Contracts;
using Module = Autofac.Module;

namespace PlexRipper.Application;

/// <summary>
/// Used to register all dependencies in Autofac for the Application project.
/// </summary>
public class ApplicationModule : Module
{
    /// <inheritdoc/>
    protected override void Load(ContainerBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // register all I*Services
        builder
            .RegisterAssemblyTypes(assembly)
            .Where(t => t.Name.EndsWith("Service"))
            .AsImplementedInterfaces()
            .SingleInstance();

        builder.RegisterType<DownloadQueue>().As<IDownloadQueue>().SingleInstance();
        builder.RegisterType<DownloadTaskScheduler>().As<IDownloadTaskScheduler>().SingleInstance();
        builder.RegisterType<FileMergeScheduler>().As<IFileMergeScheduler>().SingleInstance();
        builder.RegisterType<DownloadWorker>().InstancePerDependency();
        builder.RegisterType<PlexDownloadClient>().As<IPlexDownloadClient>().InstancePerDependency();

        builder.RegisterType<SchedulerService>().As<ISchedulerService>().SingleInstance();
        builder.RegisterType<AllJobListener>().As<IAllJobListener>().SingleInstance();
    }
}
