using System.Reflection;
using Autofac;
using Autofac.Extras.Quartz;
using Reaparr.BackgroundJobs.Contracts;
using Module = Autofac.Module;

namespace Reaparr.BackgroundJobs;

/// <summary>
/// Used to register all dependencies in Autofac for the BackgroundJobs project.
/// </summary>
public class BackgroundJobsModule : Module
{
    /// <inheritdoc/>
    protected override void Load(ContainerBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();
        builder
            .RegisterType<RefreshLibraryProgressReporter>()
            .As<IRefreshLibraryProgressReporter>()
            .InstancePerDependency();

        builder.RegisterModule(new QuartzAutofacJobsModule(assembly));
        builder.RegisterType<LibrarySyncJobListener>().As<ILibrarySyncJobListener>().SingleInstance();
    }
}
