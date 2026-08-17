namespace Reaparr.Data.Contracts;

public static partial class DbContextExtensions
{
    // SQLite has a default limit of 999 SQL parameters. Leave headroom for predicates beyond the ID list.
    private const int SQLITE_IN_CLAUSE_BATCH_SIZE = 800;

    /// <summary>
    /// Deletes rows selected by ID batches, avoiding SQLite's SQL-variable limit for large IN clauses.
    /// The caller supplies the query because the ID may be a primary key or a foreign key.
    /// </summary>
    public static async Task BulkDeleteByIdsAsync<TEntity, TKey>(
        this IReaparrDbContext dbContext,
        IEnumerable<TKey> ids,
        Func<IReaparrDbContext, TKey[], IQueryable<TEntity>> queryFactory,
        CancellationToken cancellationToken
    )
        where TEntity : class
        where TKey : notnull
    {
        foreach (var batch in ids.Distinct().Chunk(SQLITE_IN_CLAUSE_BATCH_SIZE))
            await queryFactory(dbContext, batch).ExecuteDeleteAsync(cancellationToken);
    }
}
