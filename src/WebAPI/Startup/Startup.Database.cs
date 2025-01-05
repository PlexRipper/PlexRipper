using Autofac;
using Autofac.Extensions.DependencyInjection;
using Data.Contracts;

namespace PlexRipper.WebAPI;

public static partial class StartupExtensions
{
    public static void ConfigureDatabase(this WebApplication app)
    {
        var container = app.Services.GetAutofacRoot();

        var dbContextManager = container.Resolve<IPlexRipperDbContextManager>();

        var databaseSetupResult = dbContextManager.Setup();
        if (databaseSetupResult.IsFailed)
        {
            throw new Exception(databaseSetupResult.ToString());
        }
    }
}
