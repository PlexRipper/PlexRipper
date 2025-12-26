using Autofac;
using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;

namespace Reaparr.Data;

public class DataModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder
            .Register(ctx => ctx.Resolve<IReaparrDbContextFactory>().Create())
            .As<IReaparrDbContext>()
            .InstancePerDependency();

        // For database operations (migrations, etc.), create via factory.
        // It needs to be resolved like this instead of directly resolving IReaparrDbContextFactory
        builder
            .Register(ctx =>
                (IReaparrDbContextDatabase)ctx.Resolve<IDbContextFactory<ReaparrDbContext>>().CreateDbContext()
            )
            .As<IReaparrDbContextDatabase>()
            .InstancePerDependency();

        builder.RegisterType<ReaparrDbContextManager>().As<IReaparrDbContextManager>().InstancePerDependency();

        builder.RegisterType<ReaparrDbContextFactory>().As<IReaparrDbContextFactory>().InstancePerDependency();
    }
}
