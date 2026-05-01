using FluentResults;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reaparr.Environment;

namespace Reaparr.Identity;

public sealed class AuthDbContext : IdentityDbContext<AppUser>, IAuthDbContext, IAuthDbContextDatabase
{
    private readonly IPathProvider _pathProvider;

    private readonly IAppRuntimeInfo _appRuntimeInfo;

    public string DatabaseName { get; }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    public DbSet<DownloadClientSession> DownloadClientSessions { get; set; }

    [ActivatorUtilitiesConstructor]
    public AuthDbContext(IPathProvider pathProvider, IAppRuntimeInfo appRuntimeInfo)
    {
        _pathProvider = pathProvider;
        _appRuntimeInfo = appRuntimeInfo;
        DatabaseName = pathProvider.DatabaseName;
    }

    public AuthDbContext(
        DbContextOptions<AuthDbContext> options,
        IPathProvider pathProvider,
        IAppRuntimeInfo appRuntimeInfo
    )
        : base(options)
    {
        _pathProvider = pathProvider;
        _appRuntimeInfo = appRuntimeInfo;

        DatabaseName = pathProvider.DatabaseName;
    }

    public AuthDbContext(
        DbContextOptions<AuthDbContext> options,
        IPathProvider pathProvider,
        IAppRuntimeInfo appRuntimeInfo,
        string databaseName
    )
        : base(options)
    {
        _pathProvider = pathProvider;
        _appRuntimeInfo = appRuntimeInfo;

        DatabaseName = databaseName;
        Database.OpenConnection();
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.DefaultConfiguration(_pathProvider, _appRuntimeInfo, typeof(AuthDbContext));
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
