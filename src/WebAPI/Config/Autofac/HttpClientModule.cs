using Autofac;

namespace PlexRipper.WebAPI;

public class HttpClientModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(c => c.Resolve<IHttpClientFactory>().CreateClient())
            .As<HttpClient>()
            .SingleInstance();
    }
}