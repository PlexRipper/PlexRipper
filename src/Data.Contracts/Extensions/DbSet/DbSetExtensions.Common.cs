using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbSetExtensions
{
    public static async Task<TEntity?> GetAsync<TEntity>(this DbSet<TEntity> dbSet, int id, CancellationToken cancellationToken = default)
        where TEntity : class => await dbSet.FindAsync(new object[] { id }, cancellationToken);

    public static async Task<TEntity?> GetAsync<TEntity>(this DbSet<TEntity> dbSet, Guid guid, CancellationToken cancellationToken = default)
        where TEntity : class => await dbSet.FindAsync(new object[] { guid }, cancellationToken);

    public static async Task<TEntity?> GetAsync<TEntity>(this IQueryable<TEntity> queryable, int id, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity => await queryable.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public static async Task<TEntity?> GetAsync<TEntity>(this IQueryable<TEntity> queryable, Guid guid, CancellationToken cancellationToken = default)
        where TEntity : BaseEntityGuid => await queryable.FirstOrDefaultAsync(x => x.Id == guid, cancellationToken);
}