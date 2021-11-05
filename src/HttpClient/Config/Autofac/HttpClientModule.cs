using Autofac;
using PlexRipper.Domain;

namespace PlexRipper.HttpClient.Config
{
    public class HttpClientModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<System.Net.Http.HttpClient>().SingleInstance();
            builder.RegisterType<PlexRipperHttpClient>().As<IPlexRipperHttpClient>().SingleInstance();
        }
    }
}