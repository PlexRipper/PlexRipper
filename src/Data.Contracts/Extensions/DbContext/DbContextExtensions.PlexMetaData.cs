using System.Text;
using EntityFrameworkCore.Sqlite.Concurrency;

namespace Reaparr.Data.Contracts;

public static partial class DbContextExtensions
{
    private const int CHUNK_SIZE = 500;

    public static async Task<int> InsertOrIgnorePlexActorsAsync(
        this IReaparrDbContext dbContext,
        IReadOnlyCollection<PlexActor> entities,
        CancellationToken ct)
    {
        var count = 0;

        foreach (var chunk in entities.Chunk(CHUNK_SIZE))
        {
            await ((DbContext)dbContext).ExecuteWithRetryAsync(async db =>
            {
                var query = new StringBuilder();
                foreach (var actor in chunk)
                {
                    query.AppendLine(
                        $"INSERT OR IGNORE INTO PlexActors (Name, Key) VALUES ('{Escape(actor.Name)}', '{Escape(actor.Key)}');");
                }

                count += await db.Database.ExecuteSqlRawAsync(query.ToString(), ct);

                return true;
            }, cancellationToken: ct);
        }

        return count;
    }

    public static async Task<int> InsertOrIgnorePlexGenresAsync(
        this IReaparrDbContext dbContext,
        IReadOnlyCollection<PlexGenre> entities,
        CancellationToken ct)
    {
        var count = 0;

        foreach (var chunk in entities.Chunk(CHUNK_SIZE))
        {
            await ((DbContext)dbContext).ExecuteWithRetryAsync(async db =>
            {
                var query = new StringBuilder();
                foreach (var genre in chunk)
                {
                    query.AppendLine(
                        $"INSERT OR IGNORE INTO PlexGenres (Name, Key) VALUES ('{Escape(genre.Name)}', '{Escape(genre.Key)}');");
                }

                count += await db.Database.ExecuteSqlRawAsync(query.ToString(), ct);

                return true;
            }, cancellationToken: ct);
        }

        return count;
    }

    public static async Task<int> InsertOrIgnorePlexCountriesAsync(
        this IReaparrDbContext dbContext,
        IReadOnlyCollection<PlexCountry> entities,
        CancellationToken ct)
    {
        var count = 0;

        foreach (var chunk in entities.Chunk(CHUNK_SIZE))
        {
            await ((DbContext)dbContext).ExecuteWithRetryAsync(async db =>
            {
                var query = new StringBuilder();
                foreach (var country in chunk)
                {
                    query.AppendLine(
                        $"INSERT OR IGNORE INTO PlexCountries (Name, Key) VALUES ('{Escape(country.Name)}', '{Escape(country.Key)}');");
                }

                count += await db.Database.ExecuteSqlRawAsync(query.ToString(), ct);

                return true;
            }, cancellationToken: ct);
        }

        return count;
    }

    private static string Escape(string value) => value.Replace("'", "''");
}
