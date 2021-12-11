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
            builder.RegisterType<SignalRService>().As<ISignalRService>();
            builder.RegisterType<ProgressHub>().ExternallyOwned();
            builder.RegisterType<Boot>()
                .As<IHostedService>()
                .InstancePerDependency();

            builder.RegisterType<Boot>()
                .As<IHostLifetime>()
                .InstancePerDependency();
        }
    }
}