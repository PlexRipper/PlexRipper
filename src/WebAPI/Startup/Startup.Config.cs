using Autofac;
using Autofac.Extensions.DependencyInjection;
using Settings.Contracts;

namespace PlexRipper.WebAPI;

public static partial class Startup
{
    /// <summary>
    /// Set up the PlexRipperConfig.json file.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static Result SetupConfigFile(this WebApplication app)
    {
        var container = app.Services.GetAutofacRoot();

        var configManager = container.Resolve<IConfigManager>();

        return configManager.Setup();
    }
}
