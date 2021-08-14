using Autofac;
using PlexRipper.Application.Common;

namespace PlexRipper.Settings.Config
{
    public class SettingsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<SettingsService>()
                .As<ISettingsService>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<UserSettings>()
                .As<IUserSettings>()
                .SingleInstance();
        }
    }
}