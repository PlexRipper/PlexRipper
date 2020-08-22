using Autofac;
using PlexRipper.SignalR.Hubs;

namespace PlexRipper.WebAPI.Config
{
    public class WebApiModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DownloadHub>().ExternallyOwned();
            builder.RegisterType<LibraryProgressHub>().ExternallyOwned();
        }
    }
}
