using Microsoft.EntityFrameworkCore.Design;
using Reaparr.Environment;

namespace Reaparr.Data;

public class PlexRipperDbDesignTimeContext : IDesignTimeDbContextFactory<PlexRipperDbContext>
{
    public PlexRipperDbContext CreateDbContext(string[] args) => new(PathProvider.DatabaseName);
}
