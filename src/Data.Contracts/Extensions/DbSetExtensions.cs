using Microsoft.EntityFrameworkCore;

namespace Data.Contracts;

public static class DbSetExtensions
{
    public static async Task<T?> GetAsync<T>(this DbSet<T> set, int id, CancellationToken cancellationToken = default) where T : class
    {
        return await set.FindAsync(new object[] { id }, cancellationToken);
    }
}