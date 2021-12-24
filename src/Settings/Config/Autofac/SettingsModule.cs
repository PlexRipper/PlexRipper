using Autofac;
using PlexRipper.Application;

namespace PlexRipper.Settings.Config
{
    public class SettingsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<UserSettings>().As<IUserSettings>().SingleInstance();
            builder.RegisterType<ConfigManager>().As<IConfigManager>().SingleInstance();
        }
    }
}