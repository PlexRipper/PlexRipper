using System.Reflection;
using Autofac;
using Autofac.Extras.Quartz;
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

        builder.RegisterType<DownloadQueue>().As<IDownloadQueue>().SingleInstance();
        builder.RegisterType<DownloadTaskValidator>().As<IDownloadTaskValidator>().SingleInstance();
        builder.RegisterType<DownloadTaskFactory>().As<IDownloadTaskFactory>().SingleInstance();
        builder.RegisterType<DownloadFileStream>().As<IDownloadFileStream>().SingleInstance();

        builder.RegisterModule(new QuartzAutofacJobsModule(assembly));

        // MediatR
        var configuration = MediatRConfigurationBuilder
            .Create(assembly)
            .WithAllOpenGenericHandlerTypesRegistered()
            .Build();
        builder.RegisterMediatR(configuration);
    }
}