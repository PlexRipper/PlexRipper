using Autofac;
using PlexRipper.Application;
using PlexRipper.PlexApi.Services;
using RestSharp;

namespace PlexRipper.PlexApi
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