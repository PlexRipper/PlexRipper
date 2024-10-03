using System.Collections.Specialized;
using System.Reflection;
using Autofac;
using Autofac.Extras.Quartz;
using Data.Contracts;
using Environment;
using PlexRipper.Domain.Autofac;
using Module = Autofac.Module;

namespace PlexRipper.Application;

public class QuartzModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Source: https://github.com/alphacloud/Autofac.Extras.Quartz
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
            { "quartz.dataSource.default.connectionString", DbContextConnections.ConnectionString },
        };

        // Register Quartz dependencies
        builder.RegisterModule(new QuartzAutofacFactoryModule { ConfigurationProvider = _ => quartzProps });

        // register all Quartz jobs
        builder.RegisterModule(new QuartzAutofacJobsModule(assembly));

        // Source: https://github.com/alphacloud/Autofac.Extras.Quartz/blob/develop/src/Samples/Shared/Bootstrap.cs
        builder.Register(_ => new ScopedDependency("global")).AsImplementedInterfaces().SingleInstance();
    }
}
