using Microsoft.EntityFrameworkCore.Design;

namespace PlexRipper.Infrastructure.Persistence
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PlexRipperDbContext>
    {
        public PlexRipperDbContext CreateDbContext(string[] args)
        {
            return new PlexRipperDbContext(PlexRipperDbContext.GetConfig().Options);
        }
    }
}
