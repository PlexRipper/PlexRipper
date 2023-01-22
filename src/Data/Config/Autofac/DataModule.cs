using Autofac;

namespace PlexRipper.Data;

public class DataModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<PlexRipperDbContext>()
            .InstancePerDependency();
    }
}