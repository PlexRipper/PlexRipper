using System;
using Autofac;
using Autofac.Extras.Quartz;
using PlexRipper.Application;
using PlexRipper.Domain.Autofac;
using PlexRipper.DownloadManager;

namespace PlexRipper.WebAPI.Config
{
    public class QuartzModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
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
                },
            });

            builder.RegisterModule(new QuartzAutofacJobsModule(typeof(SyncServerJob).Assembly));
            builder.RegisterModule(new QuartzAutofacJobsModule(typeof(DownloadProgressJob).Assembly));
        }
    }
}