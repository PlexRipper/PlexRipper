using System.Reflection;
using Autofac;
using Autofac.Extras.Quartz;
using BackgroundServices.Contracts;
using DownloadManager.Contracts;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using Module = Autofac.Module;

namespace PlexRipper.DownloadManager;

public class DownloadManagerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();

        builder.RegisterType<DownloadCommands>().As<IDownloadCommands>().SingleInstance();
        builder.RegisterType<DownloadQueue>().As<IDownloadQueue>().SingleInstance();
        builder.RegisterType<DownloadTaskValidator>().As<IDownloadTaskValidator>().SingleInstance();
        builder.RegisterType<DownloadTaskFactory>().As<IDownloadTaskFactory>().SingleInstance();
        builder.RegisterType<DownloadFileStream>().As<IDownloadFileStream>().SingleInstance();
        builder.RegisterType<DownloadTaskScheduler>().As<IDownloadTaskScheduler>().SingleInstance();

        builder.RegisterType<DownloadWorker>().InstancePerDependency();
        builder.RegisterType<PlexDownloadClient>().InstancePerDependency();

        builder.RegisterModule(new QuartzAutofacJobsModule(assembly));

        // MediatR
        var configuration = MediatRConfigurationBuilder
            .Create(assembly)
            .WithAllOpenGenericHandlerTypesRegistered()
            .WithRegistrationScope(RegistrationScope.Transient)
            .Build();
        builder.RegisterMediatR(configuration);
    }
}