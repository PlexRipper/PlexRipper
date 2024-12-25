using Autofac;
using PlexRipper.Identity.Contracts;

namespace PlexRipper.Identity;

public class IdentityModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AuthDbContext>().As<IAuthDbContext>().AsSelf().InstancePerDependency();

        builder.RegisterType<AuthDbContext>().As<IAuthDbContextDatabase>().InstancePerDependency();
    }
}
