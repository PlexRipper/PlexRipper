using Autofac;
using PlexRipper.Data;

namespace PlexRipper.Identity;

public class IdentityModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AppUserDbContext>().AsSelf().InstancePerDependency();
    }
}
