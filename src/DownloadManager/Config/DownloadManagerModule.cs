using Autofac;
using PlexRipper.Application.Common.Interfaces.DownloadManager;

namespace PlexRipper.DownloadManager.Config
{
    public class DownloadManagerModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Download.DownloadManager>().As<IDownloadManager>().SingleInstance();
        }
    }
}
