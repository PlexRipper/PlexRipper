using System.Runtime.CompilerServices;
using EntityFrameworkCore.Sqlite.Concurrency;

namespace Reaparr.Data.Contracts;

public static partial class DbContextExtensions
{
    private const int CHUNK_SIZE = 400;

    public static async Task<Dictionary<string, int>> InsertOrIgnorePlexActorsAsync(
        this IReaparrDbContext dbContext,
        IReadOnlyCollection<PlexActor> entities,
        CancellationToken ct)
    {
        var result = new Dictionary<string, int>(entities.Count);

        foreach (var chunk in entities.Chunk(CHUNK_SIZE))
        {
            var arguments = new List<object>(chunk.Length * 2);
            var values = new List<string>(chunk.Length);
            foreach (var actor in chunk)
            {
                values.Add($"({{{arguments.Count}}}, {{{arguments.Count + 1}}})");
                arguments.Add(actor.Name);
                arguments.Add(actor.Key);
            }

            var sql = FormattableStringFactory.Create(
                $"INSERT OR IGNORE INTO PlexActors (Name, Key) VALUES {string.Join(", ", values)}",
                arguments.ToArray()
            );

            await ((DbContext)dbContext).ExecuteWithRetryAsync(async db =>
            {
                await db.Database.ExecuteSqlInterpolatedAsync(sql, ct);
                return true;
            }, cancellationToken: ct);

            var keys = chunk.Select(a => a.Key).ToList();
            var found = await dbContext.PlexActors
                .Where(a => keys.Contains(a.Key))
                .Select(a => new { a.Key, a.Id })
                .ToListAsync(ct);
            foreach (var a in found)
                result[a.Key] = a.Id;
        }

        return result;
    }

    public static async Task<Dictionary<string, int>> InsertOrIgnorePlexGenresAsync(
        this IReaparrDbContext dbContext,
        IReadOnlyCollection<PlexGenre> entities,
        CancellationToken ct)
    {
        var result = new Dictionary<string, int>(entities.Count);

        foreach (var chunk in entities.Chunk(CHUNK_SIZE))
        {
            var arguments = new List<object>(chunk.Length * 2);
            var values = new List<string>(chunk.Length);
            foreach (var genre in chunk)
            {
                values.Add($"({{{arguments.Count}}}, {{{arguments.Count + 1}}})");
                arguments.Add(genre.Name);
                arguments.Add(genre.Key);
            }

            var sql = FormattableStringFactory.Create(
                $"INSERT OR IGNORE INTO PlexGenres (Name, Key) VALUES {string.Join(", ", values)}",
                arguments.ToArray()
            );
            await ((DbContext)dbContext).ExecuteWithRetryAsync(async db =>
            {
                await db.Database.ExecuteSqlInterpolatedAsync(sql, ct);
                return true;
            }, cancellationToken: ct);

            var keys = chunk.Select(g => g.Key).ToList();
            var found = await dbContext.PlexGenres
                .Where(g => keys.Contains(g.Key))
                .Select(g => new { g.Key, g.Id })
                .ToListAsync(ct);
            foreach (var g in found)
                result[g.Key] = g.Id;
        }

        return result;
    }

    public static async Task<Dictionary<string, int>> InsertOrIgnorePlexCountriesAsync(
        this IReaparrDbContext dbContext,
        IReadOnlyCollection<PlexCountry> entities,
        CancellationToken ct)
    {
        var result = new Dictionary<string, int>(entities.Count);

        foreach (var chunk in entities.Chunk(CHUNK_SIZE))
        {
            var arguments = new List<object>(chunk.Length * 2);
            var values = new List<string>(chunk.Length);
            foreach (var country in chunk)
            {
                values.Add($"({{{arguments.Count}}}, {{{arguments.Count + 1}}})");
                arguments.Add(country.Name);
                arguments.Add(country.Key);
            }

            var sql = FormattableStringFactory.Create(
                $"INSERT OR IGNORE INTO PlexCountries (Name, Key) VALUES {string.Join(", ", values)}",
                arguments.ToArray()
            );
            await ((DbContext)dbContext).ExecuteWithRetryAsync(async db =>
            {
                await db.Database.ExecuteSqlInterpolatedAsync(sql, ct);
                return true;
            }, cancellationToken: ct);

            var keys = chunk.Select(c => c.Key).ToList();
            var found = await dbContext.PlexCountries
                .Where(c => keys.Contains(c.Key))
                .Select(c => new { c.Key, c.Id })
                .ToListAsync(ct);
            foreach (var c in found)
                result[c.Key] = c.Id;
        }

        return result;
    }
}
