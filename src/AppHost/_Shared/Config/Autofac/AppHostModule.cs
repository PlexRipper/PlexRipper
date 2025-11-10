using Autofac;

namespace Reaparr.AppHost;

/// <summary>
///  Autofac module for the AppHost project.
/// </summary>
public class AppHostModule : Module
{
    /// <inheritdoc/>
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(c => c.Resolve<IHttpClientFactory>().CreateClient()).As<HttpClient>().InstancePerDependency();

        // This needs to be registered in order to fire Boot on Application startup
        builder.RegisterType<Boot>().As<IHostedService>().SingleInstance();
    }
}
