using Autofac;
using Reaparr.Identity.Contracts;

namespace Reaparr.Identity;

public class IdentityModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AuthDbContext>().As<IAuthDbContext>().AsSelf().InstancePerDependency();

        builder.RegisterType<AuthDbContext>().As<IAuthDbContextDatabase>().InstancePerDependency();
    }
}
