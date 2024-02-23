using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static class DbSetExtensions
{
    public static async Task<TEntity?> GetAsync<TEntity>(this DbSet<TEntity> dbSet, int id, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        return await dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public static async Task<TEntity?> GetAsync<TEntity>(this IQueryable<TEntity> queryable, int id, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        return await queryable.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}