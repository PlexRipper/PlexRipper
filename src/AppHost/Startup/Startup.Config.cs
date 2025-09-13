using Autofac;
using Autofac.Extensions.DependencyInjection;
using Reaparr.Settings.Contracts;

namespace Reaparr.AppHost;

public static partial class Startup
{
    /// <summary>
    /// Set up the ReaparrConfig.json file.
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
