using Autofac;
using Autofac.Extensions.DependencyInjection;
using Settings.Contracts;

namespace PlexRipper.WebAPI;

public static partial class Startup
{
    public static void ConfigureConfigFile(this WebApplication app)
    {
        var container = app.Services.GetAutofacRoot();

        var configManager = container.Resolve<IConfigManager>();

        var configSetupResult = configManager.Setup();
        if (configSetupResult.IsFailed)
        {
            throw new Exception(configSetupResult.ToString());
        }
    }
}
