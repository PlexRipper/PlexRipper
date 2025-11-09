using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Reaparr.Identity.Contracts;

public interface IAuthDbContext : IDataProtectionKeyContext, IDisposable
{
    #region Tables

    DbSet<DownloadClientSession> DownloadClientSessions { get; set; }

    #endregion

    EntityEntry Entry(object entity);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}