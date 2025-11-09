using Autofac;
using Reaparr.Settings.Contracts;
using Module = Autofac.Module;

namespace Reaparr.Settings.Config;

public class SettingsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<UserSettings>().As<IUserSettings>().SingleInstance();
        builder.RegisterType<ConfigManager>().As<IConfigManager>().SingleInstance();

        builder.Register(c => c.Resolve<IUserSettings>().GeneralSettings).As<IGeneralSettings>().SingleInstance();
        builder
            .Register(c => c.Resolve<IUserSettings>().AuthenticationSettings)
            .As<IAuthenticationSettings>()
            .SingleInstance();
        builder
            .Register(c => c.Resolve<IUserSettings>().ConfirmationSettings)
            .As<IConfirmationSettings>()
            .SingleInstance();
        builder.Register(c => c.Resolve<IUserSettings>().DateTimeSettings).As<IDateTimeSettings>().SingleInstance();
        builder.Register(c => c.Resolve<IUserSettings>().DisplaySettings).As<IDisplaySettings>().SingleInstance();
        builder
            .Register(c => c.Resolve<IUserSettings>().DownloadManagerSettings)
            .As<IDownloadManagerSettings>()
            .SingleInstance();
        builder.Register(c => c.Resolve<IUserSettings>().LanguageSettings).As<ILanguageSettings>().SingleInstance();
        builder.Register(c => c.Resolve<IUserSettings>().DebugSettings).As<IDebugSettings>().SingleInstance();
        builder.Register(c => c.Resolve<IUserSettings>().ServerSettings).As<IServerSettingsModule>().SingleInstance();

        builder
            .Register(c => c.Resolve<IUserSettings>().IntegrationsSettings)
            .As<IIntegrationsSettings>()
            .SingleInstance();
        builder
            .Register(c => c.Resolve<IUserSettings>().IntegrationsSettings.Sonarr)
            .As<ISonarrSettings>()
            .SingleInstance();
        builder
            .Register(c => c.Resolve<IUserSettings>().IntegrationsSettings.Radarr)
            .As<IRadarrSettings>()
            .SingleInstance();
    }
}
