using Autofac;
using Autofac.Extensions.DependencyInjection;
using Data.Contracts;

namespace PlexRipper.WebAPI;

public static partial class Startup
{
    /// <summary>
    /// Set up the database.
    /// </summary>
    public static Result SetupDatabase(this WebApplication app)
    {
        var container = app.Services.GetAutofacRoot();

        var dbContextManager = container.Resolve<IPlexRipperDbContextManager>();

        return dbContextManager.Setup();
    }
}
