using Autofac;

namespace Reaparr.PublicAPI;

/// <summary>
/// Used to register all dependencies in Autofac for the Application project.
/// </summary>
public class PublicApiModule : Module
{
    /// <inheritdoc/>
    protected override void Load(ContainerBuilder builder)
    {
        // Register all types in this assembly that implement IDownloadClientSessionManager
        builder.RegisterType<DownloadClientSessionManager>().As<IDownloadClientSessionManager>().SingleInstance();
    }
}