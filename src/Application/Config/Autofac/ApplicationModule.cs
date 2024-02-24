using System.Reflection;
using Application.Contracts;
using Autofac;
using Autofac.Extras.Quartz;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using PlexRipper.DownloadManager;
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
        builder.RegisterAssemblyTypes(assembly)
            .Where(t => t.Name.EndsWith("Service"))
            .AsImplementedInterfaces()
            .SingleInstance();

        // MediatR
        var configuration = MediatRConfigurationBuilder
            .Create(assembly)
            .WithAllOpenGenericHandlerTypesRegistered()
            .Build();
        builder.RegisterMediatR(configuration);

        // register all I*Commands
        builder.RegisterAssemblyTypes(assembly)
            .Where(t => t.Name.EndsWith("Command"))
            .AsImplementedInterfaces()
            .SingleInstance();

        // register all I*Repository
        builder.RegisterAssemblyTypes(assembly)
            .Where(t => t.Name.EndsWith("Repository"))
            .AsImplementedInterfaces()
            .SingleInstance();

        builder.RegisterType<DownloadTaskScheduler>().As<IDownloadTaskScheduler>().SingleInstance();
        builder.RegisterType<DownloadWorker>().InstancePerDependency();
        builder.RegisterType<PlexDownloadClient>().InstancePerDependency();

        // register all Quartz jobs
        builder.RegisterModule(new QuartzAutofacJobsModule(assembly));
    }
}