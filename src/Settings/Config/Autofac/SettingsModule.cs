using System.Reflection;
using Autofac;
using Settings.Contracts;
using Module = Autofac.Module;

namespace PlexRipper.Settings.Config;

public class SettingsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<UserSettings>().As<IUserSettings>().SingleInstance();
        builder.RegisterType<ConfigManager>().As<IConfigManager>().SingleInstance();

        var assembly = Assembly.GetExecutingAssembly();

        // register all I*SettingsModule
        builder.RegisterAssemblyTypes(assembly)
            .Where(t => t.Name.EndsWith("SettingsModule"))
            .AsImplementedInterfaces()
            .SingleInstance();
    }
}