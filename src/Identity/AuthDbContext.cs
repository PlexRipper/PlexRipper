using Data.Contracts;
using FluentResults;
using Logging;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlexRipper.Identity.Contracts;

namespace PlexRipper.Identity;

public class AuthDbContext : IdentityDbContext<AppUser>, IAuthDbContext, IAuthDbContextDatabase
{
    public AuthDbContext() { }

    public AuthDbContext(DbContextOptions<AuthDbContext> options)
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
                    b.MigrationsAssembly(typeof(AuthDbContext).Assembly.FullName);
                }
            );
        }
    }

    /// <inheritdoc/>
    public bool CanConnect() => Database.CanConnect();

    /// <inheritdoc/>
    public bool IsInMemory() => Database.IsInMemory();

    /// <inheritdoc/>
    public void CloseConnection() => Database.CloseConnection();

    /// <inheritdoc/>
    public Result<bool> EnsureDeleted()
    {
        try
        {
            return Result.Ok(Database.EnsureDeleted());
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e));
        }
    }

    /// <inheritdoc/>
    public Result Migrate()
    {
        try
        {
            Database.Migrate();
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e));
        }
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetPendingMigrations() => Database.GetPendingMigrations();
}
