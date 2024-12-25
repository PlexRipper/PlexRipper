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
    public string DatabaseName { get; } = string.Empty;

    public AuthDbContext() { }

    public AuthDbContext(string databaseName)
    {
        DatabaseName = databaseName;
    }

    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options) { }

    public AuthDbContext(DbContextOptions<AuthDbContext> options, string databaseName)
        : base(options)
    {
        DatabaseName = databaseName;
        Database.OpenConnection();
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.DefaultConfiguration(typeof(AuthDbContext));
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
