using Autofac;
using Autofac.Extensions.DependencyInjection;
using Settings.Contracts;

namespace PlexRipper.WebAPI;

public static partial class Startup
{
    public static Result ConfigureConfigFile(this WebApplication app)
    {
        var container = app.Services.GetAutofacRoot();

        var configManager = container.Resolve<IConfigManager>();

        return configManager.Setup();
    }
}
