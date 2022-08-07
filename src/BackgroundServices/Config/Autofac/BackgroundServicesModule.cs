using System.Reflection;
using Autofac;
using Autofac.Extras.Quartz;
using BackgroundServices.DownloadManager.Jobs;
using BackgroundServices.Jobs;
using PlexRipper.Application;
using PlexRipper.Domain.Autofac;
using Module = Autofac.Module;

namespace BackgroundServices;

/// <summary>
/// Used to register all dependancies in Autofac for the Application project.
/// </summary>
public class BackgroundServicesModule : Module
{
    /// <inheritdoc/>
    protected override void Load(ContainerBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Source: https://github.com/alphacloud/Autofac.Extras.Quartz/blob/develop/src/Samples/Shared/Bootstrap.cs
        builder.Register(_ => new ScopedDependency("global"))
            .AsImplementedInterfaces()
            .SingleInstance();

        // Register Quartz dependancies
        builder.RegisterModule(new QuartzAutofacFactoryModule
        {
            JobScopeConfigurator = (cb, tag) =>
            {
                // override dependency for job scope
                cb.Register(_ => new ScopedDependency("job-local " + DateTime.UtcNow.ToLongTimeString()))
                    .AsImplementedInterfaces()
                    .InstancePerMatchingLifetimeScope(tag);
            }
        });

        builder.RegisterModule(new QuartzAutofacJobsModule(typeof(SyncServerJob).Assembly));
        builder.RegisterModule(new QuartzAutofacJobsModule(typeof(DownloadProgressJob).Assembly));

        builder.RegisterType<SchedulerService>().As<ISchedulerService>().SingleInstance();

        //builder.RegisterType<DownloadScheduler>().As<IDownloadScheduler>().SingleInstance();
        //builder.RegisterType<DownloadProgressScheduler>().As<IDownloadProgressScheduler>().SingleInstance();

        // register all I*Scheduler
        builder.RegisterAssemblyTypes(assembly)
            .Where(t => t.Name.EndsWith("Scheduler"))
            .AsImplementedInterfaces()
            .SingleInstance();
    }
}