using Microsoft.EntityFrameworkCore.Design;

namespace Reaparr.Identity;

public class AuthDbDesignTimeContext : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        IAppRuntimeInfo appRuntimeInfo = new AppRuntimeInfo();

        // NOTE: EF Core design-time runs under `dotnet-ef` entry assembly,
        // so AppBuildInfo cannot read Reaparr assembly metadata (RuntimeMode).
        // Force desktop mode for design-time path resolution to avoid
        // PlatformNotSupportedException on Linux hosts.
        IAppBuildInfo designTimeBuildInfo = new DesignTimeAppBuildInfo();
        IPathProvider pathProvider = new PathProvider(designTimeBuildInfo, appRuntimeInfo);

        return new AuthDbContext(Log.Logger, pathProvider, appRuntimeInfo);
    }
}
