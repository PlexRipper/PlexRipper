using Microsoft.EntityFrameworkCore.Design;

namespace Reaparr.Data;

public class ReaparrDbDesignTimeContext : IDesignTimeDbContextFactory<ReaparrDbContext>
{
    public ReaparrDbContext CreateDbContext(string[] args)
    {
        IPathProvider pathProvider = new PathProvider();
        return new(pathProvider);
    }
}
