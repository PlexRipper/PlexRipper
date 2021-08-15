using Autofac;
using PlexRipper.Application.Common;
using PlexRipper.DownloadManager.Download;

namespace PlexRipper.DownloadManager.Common.Autofac
{
    public class DownloadManagerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DownloadManager>().As<IDownloadManager>().SingleInstance();
            builder.RegisterType<DownloadQueue>().As<IDownloadQueue>().SingleInstance();
            builder.RegisterType<DownloadWorker>().InstancePerDependency();
        }
    }
}