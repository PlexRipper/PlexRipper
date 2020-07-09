using Autofac;
using Microsoft.AspNetCore.SignalR;
using System.Reflection;

namespace PlexRipper.SignalR.Config
{
    public class SignalRModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => typeof(Hub).IsAssignableFrom(t))
                .ExternallyOwned();
        }
    }
}
