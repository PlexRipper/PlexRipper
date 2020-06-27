using Autofac;
using PlexRipper.Application.Common.Interfaces.Settings;
using Serilog;


namespace PlexRipper.Settings.Config
{
    public class SettingsModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(x => new UserSettings(x.Resolve<ILogger>()))
                .As<IUserSettings>()
                .SingleInstance();
        }
    }
}
