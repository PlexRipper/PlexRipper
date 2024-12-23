using Data.Contracts;
using Logging;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlexRipper.Domain;
using PlexRipper.Identity.Contracts;

namespace PlexRipper.Data;

public class AppUserDbContext : IdentityDbContext<AppUser>
{
    public AppUserDbContext() { }

    public AppUserDbContext(DbContextOptions<AppUserDbContext> options)
        : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Source: https://github.com/tompazourek/NaturalSort.Extension
            SqliteConnection databaseConnection = new(DbContextConnections.ConnectionString);

            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            optionsBuilder.LogTo(text => LogManager.DbContextLogger(text), LogLevel.Error);
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.UseSqlite(
                databaseConnection,
                b =>
                {
                    // Wait as long as needed for the database to be unlocked
                    b.CommandTimeout(300);
                    b.MigrationsAssembly(typeof(AppUserDbContext).Assembly.FullName);
                }
            );
        }
    }
}
