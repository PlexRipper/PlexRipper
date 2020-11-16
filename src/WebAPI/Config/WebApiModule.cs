using Autofac;
using Microsoft.Extensions.Hosting;
using PlexRipper.SignalR.Hubs;

namespace PlexRipper.WebAPI.Config
{
    public class WebApiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
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