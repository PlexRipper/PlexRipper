﻿using System.Collections.Specialized;
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
            { "quartz.serializer.type", "stj" },
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
            // False because we are first setting up autofac, and then the database. When the database is set up, the schema is already validated.
            { "quartz.jobStore.performSchemaValidation", "false" },
            { "quartz.dataSource.default.connectionString", DbContextConnections.ConnectionString },
        };

        // Register Quartz dependencies
        builder.RegisterModule(new QuartzAutofacFactoryModule { ConfigurationProvider = _ => quartzProps });

        // register all Quartz jobs
        builder.RegisterModule(new QuartzAutofacJobsModule(assembly));

        // Source: https://github.com/alphacloud/Autofac.Extras.Quartz/blob/develop/src/Samples/Shared/Bootstrap.cs
        builder.Register(_ => new ScopedDependency("global")).AsImplementedInterfaces().SingleInstance();
    }

    public static NameValueCollection TestQuartzConfiguration() =>
        // During integration testing, we cannot use a real JobStore so we revert to default
        new()
        {
            // The unique identifier for the scheduler is needed to prevent conflicts when running multiple schedulers in integration tests
            { "quartz.scheduler.instanceName", "TestPlexRipper_Scheduler" + Guid.NewGuid() },
            { "quartz.scheduler.instanceId", Guid.NewGuid().ToString() },
            { "quartz.serializer.type", "stj" },
            { "quartz.jobStore.type", "Quartz.Simpl.RAMJobStore, Quartz" },
            { "quartz.threadPool.type", "Quartz.Simpl.SimpleThreadPool, Quartz" },
            { "quartz.threadPool.threadCount", "10" },
            { "quartz.jobStore.misfireThreshold", "60000" },
        };
}
