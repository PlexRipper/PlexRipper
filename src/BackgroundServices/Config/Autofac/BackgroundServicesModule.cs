using System.Collections.Specialized;
using System.Reflection;
using Autofac;
using Autofac.Extras.Quartz;
using BackgroundServices.Contracts;
using Environment;
using PlexRipper.Domain.Autofac;
using Module = Autofac.Module;

namespace BackgroundServices;

/// <summary>
/// Used to register all dependencies in Autofac for the Application project.
/// </summary>
public class BackgroundServicesModule : Module
{
    /// <inheritdoc/>
    protected override void Load(ContainerBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();

        if (!EnvironmentExtensions.IsIntegrationTestMode())
        {
            var quartzProps = new NameValueCollection
            {
                { "quartz.scheduler.instanceName", "PlexRipper Scheduler" },
                { "quartz.serializer.type", "json" },
                { "quartz.threadPool.type", "Quartz.Simpl.SimpleThreadPool, Quartz" },
                { "quartz.threadPool.threadCount", "10" },
                { "quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz" },
                { "quartz.jobStore.misfireThreshold", "60000" },
                { "quartz.jobStore.lockHandler.type", "Quartz.Impl.AdoJobStore.UpdateLockRowSemaphore, Quartz" },
                { "quartz.jobStore.dataSource", "default" },
                { "quartz.jobStore.tablePrefix", QuartzDatabaseConfig.Prefix },

                // { "quartz.jobStore.useProperties", "true" },
                { "quartz.jobStore.driverDelegateType", "Quartz.Impl.AdoJobStore.SQLiteDelegate, Quartz" },
                { "quartz.dataSource.default.provider", "SQLite-Microsoft" },
                { "quartz.dataSource.default.connectionString", PathProvider.DatabaseConnectionString },
            };

            // Register Quartz dependencies
            builder.RegisterModule(new QuartzAutofacFactoryModule
            {
                // JobScopeConfigurator = (cb, tag) =>
                // {
                //     // override dependency for job scope
                //     cb.Register(_ => new ScopedDependency("job-local " + DateTime.UtcNow.ToLongTimeString()))
                //         .AsImplementedInterfaces()
                //         .InstancePerMatchingLifetimeScope(tag);
                // },

                // During integration testing, we cannot use a real JobStore so we revert to default
                ConfigurationProvider = _ => quartzProps,
            });

            // Source: https://github.com/alphacloud/Autofac.Extras.Quartz/blob/develop/src/Samples/Shared/Bootstrap.cs
            builder.Register(_ => new ScopedDependency("global"))
                .AsImplementedInterfaces()
                .SingleInstance();
        }

        builder.RegisterType<SchedulerService>().As<ISchedulerService>().InstancePerDependency();

        // register all I*Scheduler
        builder.RegisterAssemblyTypes(assembly)
            .Where(t => t.Name.EndsWith("Scheduler"))
            .AsImplementedInterfaces()
            .InstancePerDependency();

        // register all I*Listener
        builder.RegisterAssemblyTypes(assembly)
            .Where(t => t.Name.EndsWith("Listener"))
            .AsImplementedInterfaces()
            .InstancePerDependency();
    }
}