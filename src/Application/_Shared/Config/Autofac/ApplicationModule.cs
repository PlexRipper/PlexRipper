using Autofac;
using Reaparr.Application.Contracts;
using Reaparr.FileSystem.Contracts;
using Module = Autofac.Module;

namespace Reaparr.Application;

/// <summary>
/// Used to register all dependencies in Autofac for the Application project.
/// </summary>
public class ApplicationModule : Module
{
    /// <inheritdoc/>
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<DownloadQueue>().As<IDownloadQueue>().SingleInstance();
        builder.RegisterType<DownloadTaskScheduler>().As<IDownloadTaskScheduler>().SingleInstance();
        builder.RegisterType<FileMergeScheduler>().As<IFileMergeScheduler>().SingleInstance();
        builder.RegisterType<FileMergeQueue>().As<IFileMergeQueue>().SingleInstance();
        builder.RegisterType<DownloadWorker>().InstancePerDependency();
        builder.RegisterType<PlexDownloadClient>().As<IPlexDownloadClient>().InstancePerDependency();

        builder.RegisterType<SchedulerService>().As<ISchedulerService>().SingleInstance();
        builder.RegisterType<AllJobListener>().As<IAllJobListener>().SingleInstance();
        builder.RegisterType<DownloadJobListener>().As<IDownloadJobListener>().SingleInstance();
        builder.RegisterType<FileMergeJobListener>().As<IFileMergeJobListener>().SingleInstance();

        builder
            .RegisterType<RefreshLibraryProgressReporter>()
            .As<IRefreshLibraryProgressReporter>()
            .InstancePerDependency();

        // SignalR
        builder.RegisterType<SignalRService>().As<ISignalRService>();
        builder.RegisterType<ProgressHub>().ExternallyOwned();
        builder.RegisterType<NotificationHub>().ExternallyOwned();
    }
}
