using Autofac;
using PlexRipper.Application.Common;

namespace PlexRipper.SignalR.Config
{
    public class SignalRModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SignalRService>().As<ISignalRService>();

        }
    }
}
