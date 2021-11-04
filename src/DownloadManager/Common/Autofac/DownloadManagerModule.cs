using Autofac;
using PlexRipper.Application.Common;
using PlexRipper.DownloadManager.Download;

namespace PlexRipper.DownloadManager
{
    public class DownloadManagerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DownloadManager>().As<IDownloadManager>().SingleInstance();
            builder.RegisterType<DownloadCommands>().As<IDownloadCommands>().SingleInstance();
            builder.RegisterType<DownloadQueue>().As<IDownloadQueue>().SingleInstance();
            builder.RegisterType<DownloadTracker>().As<IDownloadTracker>().SingleInstance();
            builder.RegisterType<DownloadTaskValidator>().As<IDownloadTaskValidator>().SingleInstance();

            builder.RegisterType<DownloadWorker>().InstancePerDependency();
            builder.RegisterType<PlexDownloadClient>().InstancePerDependency();
        }
    }
}