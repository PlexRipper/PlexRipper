using Autofac;
using PlexRipper.Application.Common;

namespace PlexRipper.DownloadManager.Config
{
    public class DownloadManagerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DownloadManager>().As<IDownloadManager>().SingleInstance();
            builder.RegisterType<DownloadQueue>().SingleInstance();
        }
    }
}