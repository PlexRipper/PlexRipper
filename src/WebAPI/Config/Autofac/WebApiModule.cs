using Autofac;
using Microsoft.Extensions.Hosting;
using PlexRipper.Application;
using PlexRipper.WebAPI.SignalR;
using PlexRipper.WebAPI.SignalR.Hubs;

namespace PlexRipper.WebAPI.Config
{
    public class WebApiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Boot>()
                .As<IBoot>()
                .SingleInstance();

            // builder.RegisterType<Boot>()
            //     .As<IHostLifetime>()
            //     .SingleInstance();

            // SignalR
            builder.RegisterType<SignalRService>().As<ISignalRService>();
            builder.RegisterType<ProgressHub>().ExternallyOwned();
            builder.RegisterType<NotificationHub>().ExternallyOwned();
        }
    }
}