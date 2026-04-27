using Microsoft.EntityFrameworkCore.Design;

namespace Reaparr.Data;

public class ReaparrDbDesignTimeContext : IDesignTimeDbContextFactory<ReaparrDbContext>
{
    public ReaparrDbContext CreateDbContext(string[] args)
    {
        var appBuildInfo = new AppBuildInfo();
        IPathProvider pathProvider = new PathProvider(appBuildInfo);
        return new(pathProvider);
    }
}
