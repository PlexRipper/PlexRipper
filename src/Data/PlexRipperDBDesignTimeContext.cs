using Environment;
using Microsoft.EntityFrameworkCore.Design;

namespace PlexRipper.Data;

public class PlexRipperDbDesignTimeContext : IDesignTimeDbContextFactory<PlexRipperDbContext>
{
    public PlexRipperDbContext CreateDbContext(string[] args) => new(PathProvider.DatabaseName);
}
