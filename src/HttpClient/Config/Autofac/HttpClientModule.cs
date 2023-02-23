using Autofac;

namespace PlexRipper.HttpClient;

public class HttpClientModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(c => c.Resolve<IHttpClientFactory>().CreateClient())
            .As<System.Net.Http.HttpClient>()
            .SingleInstance();
    }
}