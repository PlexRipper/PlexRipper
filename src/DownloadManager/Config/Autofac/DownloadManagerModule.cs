using System.Reflection;
using Autofac;
using PlexRipper.Application;
using PlexRipper.DownloadManager.DownloadClient;
using Module = Autofac.Module;

namespace PlexRipper.DownloadManager
{
    public class DownloadManagerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DownloadCommands>().As<IDownloadCommands>().SingleInstance();
            builder.RegisterType<DownloadQueue>().As<IDownloadQueue>().SingleInstance();
            builder.RegisterType<DownloadTracker>().As<IDownloadTracker>().SingleInstance();
            builder.RegisterType<DownloadTaskValidator>().As<IDownloadTaskValidator>().SingleInstance();
            builder.RegisterType<DownloadSubscriptions>().As<IDownloadSubscriptions>().SingleInstance();
            builder.RegisterType<DownloadTaskFactory>().As<IDownloadTaskFactory>().SingleInstance();
            builder.RegisterType<DownloadFileStream>().As<IDownloadFileStream>().SingleInstance();

            builder.RegisterType<DownloadProgressNotifier>().As<IDownloadProgressNotifier>().InstancePerDependency();

            builder.RegisterType<DownloadWorker>().InstancePerDependency();
            builder.RegisterType<PlexDownloadClient>().InstancePerDependency();
        }
    }
}