using Application.Contracts;
using Autofac;

namespace PlexRipper.WebAPI;

/// <summary>
///  Autofac module for the WebAPI project.
/// </summary>
public class WebApiModule : Module
{
    /// <inheritdoc/>
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(c => c.Resolve<IHttpClientFactory>().CreateClient()).As<HttpClient>().InstancePerDependency();

        // This needs to be registered in order to fire Boot on Application startup
        builder.RegisterType<Boot>().As<IHostedService>().SingleInstance();

        // SignalR
        builder.RegisterType<SignalRService>().As<ISignalRService>();
        builder.RegisterType<ProgressHub>().ExternallyOwned();
        builder.RegisterType<NotificationHub>().ExternallyOwned();
    }
}