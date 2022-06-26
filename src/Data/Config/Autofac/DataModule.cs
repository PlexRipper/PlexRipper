using Autofac;
using PlexRipper.Application;

namespace PlexRipper.Data;

public class DataModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<PlexRipperDbContext>()
            .InstancePerDependency();

        builder.RegisterType<PlexRipperDatabaseService>().As<IPlexRipperDatabaseService>()
            .InstancePerDependency();
    }
}