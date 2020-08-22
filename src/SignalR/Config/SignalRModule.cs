using Autofac;
using Microsoft.AspNetCore.SignalR;
using System.Reflection;
using PlexRipper.Application.Common.Interfaces.SignalR;

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
