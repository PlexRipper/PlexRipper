using Autofac;

namespace Reaparr.PlexApi;

public class PlexApiModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<PlexApiClient>().As<IPlexApiClient>().InstancePerDependency();

        builder.RegisterType<PlexApiClientFactory>().As<IPlexApiClientFactory>().InstancePerLifetimeScope();
    }
}
