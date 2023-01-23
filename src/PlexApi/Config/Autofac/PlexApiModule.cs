using Autofac;
using PlexApi.Contracts;
using PlexRipper.Application;
using PlexRipper.PlexApi.Services;

namespace PlexRipper.PlexApi;

public class PlexApiModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<PlexApiService>().As<IPlexApiService>();

        builder.RegisterType<Api.PlexApi>();

        builder.RegisterType<PlexApiClient>().SingleInstance();
    }
}