using Autofac;
using Reaparr.SignalR.Contracts;

namespace Reaparr.SignalR;

/// <summary>
/// Used to register all dependencies in Autofac for the Application project.
/// </summary>
public class SignalrModule : Module
{
    /// <inheritdoc/>
    protected override void Load(ContainerBuilder builder)
    {
        // SignalR
        builder.RegisterType<ProgressHubService>().As<IProgressHubService>();
        builder.RegisterType<DownloadHubService>().As<IDownloadHubService>();
        builder.RegisterType<NotificationHubService>().As<INotificationHubService>();
        builder.RegisterType<ProgressHub>().ExternallyOwned();
        builder.RegisterType<DownloadHub>().ExternallyOwned();
        builder.RegisterType<NotificationHub>().ExternallyOwned();
    }
}
