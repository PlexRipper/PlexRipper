using Autofac;
using PlexApi.Contracts;

namespace PlexRipper.PlexApi;

public class PlexApiModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<PlexApiService>().As<IPlexApiService>();

        builder.RegisterType<PlexApiWrapper>();

        builder.RegisterType<PlexApiClient>().InstancePerDependency();
    }
}
