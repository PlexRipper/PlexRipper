using System.Collections.Specialized;
using System.Reflection;
using Autofac.Extras.Quartz;
using Microsoft.Data.Sqlite;
using Quartz;
using Quartz.Impl.AdoJobStore;
using Reaparr.Domain.Autofac;
using Module = Autofac.Module;

namespace Reaparr.Application;

public class QuartzModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register Quartz dependencies
        // Source: https://www.quartz-scheduler.net/
        builder.RegisterType<AllJobListener>().SingleInstance();
        builder.RegisterType<DownloadJobListener>().SingleInstance();
        builder.RegisterType<LibrarySyncJobListener>().SingleInstance();
        builder.RegisterType<ReaparrSchedulerListener>().SingleInstance();
        builder.RegisterType<BackgroundJobsSetup>().As<IBackgroundJobsSetup>().SingleInstance();

        // Source: https://github.com/alphacloud/Autofac.Extras.Quartz
        builder.RegisterModule(
            new QuartzAutofacFactoryModule
            {
                ConfigurationProvider = context =>
                {
                    var pathProvider = context.Resolve<IPathProvider>();
                    var connectionString = DbContextConnections.GetConnectionString(
                        pathProvider.DatabasePath,
                        SqliteOpenMode.ReadWriteCreate
                    );

                    var schedulerBuilder = SchedulerBuilder
                        .Create()
                        .WithName("Reaparr Scheduler")
                        .UseDefaultThreadPool(10)
                        .UsePersistentStore(store =>
                        {
                            // https://www.quartz-scheduler.net/documentation/quartz-3.x/packages/system-text-json.html#configuring
                            store.UseProperties = true;
                            store.UseSystemTextJsonSerializer();
                            // False because Autofac is initialized before the database. By the time the database is set up, the schema has already been validated.
                            store.PerformSchemaValidation = false;
                            store.RetryInterval = TimeSpan.FromMinutes(2);
                            store.UseGenericDatabase<SQLiteDelegate>(
                                provider: "SQLite-Microsoft",
                                dataSourceName: "default",
                                configurer: database =>
                                {
                                    database.ConnectionString = connectionString;
                                    database.TablePrefix = "QRTZ_";
                                    database.UseConnectionProvider<QuartzSqliteConnectionProvider>();
                                }
                            );
                        });

                    schedulerBuilder.InterruptJobsOnShutdownWithWait = true;
                    schedulerBuilder.MisfireThreshold = TimeSpan.FromMinutes(5);

                    return schedulerBuilder.Properties;
                },
            }
        );

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
            { "quartz.scheduler.instanceName", "TestReaparr_Scheduler" + Guid.NewGuid() },
            { "quartz.scheduler.instanceId", Guid.NewGuid().ToString() },
            { "quartz.serializer.type", "stj" },
            { "quartz.jobStore.type", "Quartz.Simpl.RAMJobStore, Quartz" },
            { "quartz.threadPool.type", "Quartz.Simpl.SimpleThreadPool, Quartz" },
            { "quartz.threadPool.threadCount", "10" },
            { "quartz.jobStore.misfireThreshold", "60000" },
        };
}
