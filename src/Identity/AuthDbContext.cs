using FluentResults;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;
using Reaparr.Identity.Contracts;

namespace Reaparr.Identity;

public sealed class AuthDbContext : IdentityDbContext<AppUser>, IAuthDbContext, IAuthDbContextDatabase
{
    public string DatabaseName { get; } = string.Empty;

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    public DbSet<DownloadClientSession> DownloadClientSessions { get; set; }

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
    public Result Migrate() => Result.Try(() => Database.Migrate(), e => new ExceptionalError(e));

    /// <inheritdoc/>
    public IEnumerable<string> GetPendingMigrations() => Database.GetPendingMigrations();
}
