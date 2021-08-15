using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PlexRipper.Data
{
    /// <summary>
    /// Used when migrating the DBContext
    /// </summary>
    public class PlexRipperDBContextDesignTimeFactory : IDesignTimeDbContextFactory<PlexRipperDbContext>
    {
        public PlexRipperDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PlexRipperDbContext>();
            optionsBuilder.UseSqlite("Data Source=PlexRipperDB.db");

            return new PlexRipperDbContext(optionsBuilder.Options);
        }
    }
}