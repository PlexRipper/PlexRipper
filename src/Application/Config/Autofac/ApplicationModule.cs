using System.Collections.Specialized;
using System.Reflection;
using Application.Contracts;
using Autofac;
using Autofac.Extras.Quartz;
using Environment;
using FileSystem.Contracts;
using FluentValidation;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using PlexRipper.Domain.Autofac;
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

        // register all FluentValidators
        builder.RegisterAssemblyTypes(assembly)
            .Where(t => t.IsClosedTypeOf(typeof(IValidator<>)))
            .AsImplementedInterfaces();

        builder.RegisterType<DownloadQueue>().As<IDownloadQueue>().SingleInstance();
        builder.RegisterType<DownloadTaskScheduler>().As<IDownloadTaskScheduler>().SingleInstance();
        builder.RegisterType<FileMergeScheduler>().As<IFileMergeScheduler>().SingleInstance();
        builder.RegisterType<DownloadWorker>().InstancePerDependency();
        builder.RegisterType<PlexDownloadClient>().As<IPlexDownloadClient>().InstancePerDependency();

        // Setup Quartz
        SetupQuartz(builder, assembly);

        builder.RegisterType<SchedulerService>().As<ISchedulerService>().InstancePerDependency();
        builder.RegisterType<AllJobListener>().As<IAllJobListener>().InstancePerDependency();
    }

    private void SetupQuartz(ContainerBuilder builder, Assembly assembly)
    {
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
            { "quartz.dataSource.default.connectionString", PathProvider.DatabaseConnectionString },
        };

        // Register Quartz dependencies
        builder.RegisterModule(new QuartzAutofacFactoryModule
        {
            ConfigurationProvider = _ => quartzProps,
        });

        // register all Quartz jobs
        builder.RegisterModule(new QuartzAutofacJobsModule(assembly));

        // Source: https://github.com/alphacloud/Autofac.Extras.Quartz/blob/develop/src/Samples/Shared/Bootstrap.cs
        builder.Register(_ => new ScopedDependency("global"))
            .AsImplementedInterfaces()
            .SingleInstance();
    }
}