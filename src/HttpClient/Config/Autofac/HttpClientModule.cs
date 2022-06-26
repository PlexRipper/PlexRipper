using Autofac;

namespace PlexRipper.HttpClient
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