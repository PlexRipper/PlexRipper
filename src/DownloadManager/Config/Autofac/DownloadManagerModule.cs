using System.Reflection;
using Autofac;
using Autofac.Extras.Quartz;
using PlexRipper.Application.Common;
using PlexRipper.DownloadManager.DownloadClient;
using Module = Autofac.Module;

namespace PlexRipper.DownloadManager
{
    public class DownloadManagerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assembly = Assembly.GetExecutingAssembly();

            builder.RegisterType<DownloadManager>().As<IDownloadManager>().SingleInstance();
            builder.RegisterType<DownloadCommands>().As<IDownloadCommands>().SingleInstance();
            builder.RegisterType<DownloadQueue>().As<IDownloadQueue>().SingleInstance();
            builder.RegisterType<DownloadTracker>().As<IDownloadTracker>().SingleInstance();
            builder.RegisterType<DownloadTaskValidator>().As<IDownloadTaskValidator>().SingleInstance();
            builder.RegisterType<DownloadScheduler>().As<IDownloadScheduler>().SingleInstance();

            builder.RegisterType<DownloadWorker>().InstancePerDependency();
            builder.RegisterType<PlexDownloadClient>().InstancePerDependency();

            // Register Quartz dependancies
            builder.RegisterModule(new QuartzAutofacFactoryModule());
            builder.RegisterModule(new QuartzAutofacJobsModule(assembly));
        }
    }
}