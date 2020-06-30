using Autofac;
using PlexRipper.Application.Common.Interfaces.PlexApi;
using PlexRipper.PlexApi.Services;

namespace PlexRipper.PlexApi.Config
{
    public class PlexApiModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PlexApiService>().As<IPlexApiService>();

            builder.RegisterType<Api.PlexApi>();

            builder.RegisterType<PlexWebClient>().SingleInstance();
        }
    }
}
