using Autofac;
using PlexRipper.Application.Common.Interfaces.DownloadManager;
using PlexRipper.Application.Common.Interfaces.Settings;
using PlexRipper.Settings.Models;


namespace PlexRipper.Settings.Config
{
    public class SettingsModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<UserSettings>()
                .As<IUserSettings>()
                .SingleInstance();

            builder.RegisterType<DownloadManagerModel>().As<IDownloadManagerModel>();

        }
    }
}
