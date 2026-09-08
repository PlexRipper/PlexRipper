using Microsoft.EntityFrameworkCore.Design;

namespace Reaparr.Data;

public class ReaparrDbDesignTimeContext : IDesignTimeDbContextFactory<ReaparrDbContext>
{
    public ReaparrDbContext CreateDbContext(string[] args)
    {
        IAppRuntimeInfo appRuntimeInfo = new AppRuntimeInfo();

        // NOTE: EF Core design-time runs under `dotnet-ef` entry assembly,
        // so AppBuildInfo cannot read Reaparr assembly metadata (RuntimeMode).
        // Force desktop mode for design-time path resolution to avoid
        // PlatformNotSupportedException on Linux hosts.
        IAppBuildInfo designTimeBuildInfo = new DesignTimeAppBuildInfo();
        IPathProvider pathProvider = new PathProvider(designTimeBuildInfo, appRuntimeInfo);

        return new ReaparrDbContext(Log.Logger, pathProvider, appRuntimeInfo);
    }
}
