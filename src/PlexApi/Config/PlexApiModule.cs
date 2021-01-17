using Autofac;
using PlexRipper.Application.Common;
using PlexRipper.PlexApi.Services;
using RestSharp;

namespace PlexRipper.PlexApi.Config
{
    public class PlexApiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PlexApiService>().As<IPlexApiService>();

            builder.RegisterType<Api.PlexApi>();

            builder.RegisterType<PlexApiClient>().SingleInstance();
            builder.RegisterType<RestClient>().As<IRestClient>().SingleInstance();
        }
    }
}