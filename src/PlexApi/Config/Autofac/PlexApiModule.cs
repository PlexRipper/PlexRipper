using Autofac;
using PlexApi.Contracts;

namespace PlexRipper.PlexApi;

public class PlexApiModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<PlexApiClient>().As<IPlexApiClient>().InstancePerDependency();

        builder.RegisterType<PlexApiMediaService>().As<IPlexApiMediaService>().InstancePerDependency();

        builder.RegisterType<PlexApiClientFactory>().As<IPlexApiClientFactory>().InstancePerLifetimeScope();
    }
}
