using Autofac;

namespace Reaparr.SignalR;

/// <summary>
/// Used to register all Autofac dependencies for the SignalR project.
/// </summary>
public class SignalrModule : Module
{
    /// <inheritdoc/>
    protected override void Load(ContainerBuilder builder)
    {
        // SignalR
        builder.RegisterType<ProgressHubService>().As<IProgressHubService>().SingleInstance();
        builder.RegisterType<DownloadHubService>().As<IDownloadHubService>().SingleInstance();
        builder.RegisterType<NotificationHubService>().As<INotificationHubService>().SingleInstance();
        builder.RegisterType<ProgressHub>().ExternallyOwned();
        builder.RegisterType<DownloadHub>().ExternallyOwned();
        builder.RegisterType<NotificationHub>().ExternallyOwned();
    }
}
