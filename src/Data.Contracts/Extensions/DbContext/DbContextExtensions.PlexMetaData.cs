using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Reaparr.Data.Contracts;

public static partial class DbContextExtensions
{
    private const int CHUNK_SIZE = 400;

    /// <summary>
    /// Uses a single SQL statement to insert or ignore multiple <see cref="PlexActor"/> entities into the database, and returns a dictionary mapping each actor's key to its corresponding ID in the database.
    /// </summary>
    /// <param name="dbContext">The database context to use for the operation.</param>
    /// <param name="entities">A read-only collection of PlexActor entities to insert or ignore.</param>
    /// <param name="ct">A cancellation token to observe while performing the operation.</param>
    /// <returns>A dictionary mapping each actor's key to its corresponding ID in the database.</returns>
    public static async Task<Dictionary<string, int>> InsertOrIgnorePlexActorsAsync(
        this IReaparrDbContext dbContext,
        IReadOnlyCollection<PlexActor> entities,
        CancellationToken ct
    )
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

            await dbContext.ExecuteSqlInterpolatedAsync(sql, ct);

            var keys = chunk.Select(a => a.Key).ToList();
            var found = await dbContext
                .PlexActors.Where(a => keys.Contains(a.Key))
                .Select(a => new { a.Key, a.Id })
                .ToListAsync(ct);
            foreach (var a in found)
                result[a.Key] = a.Id;
        }

        return result;
    }

    /// <summary>
    /// Inserts video genres that are not already present and returns their database IDs keyed by Plex key.
    /// </summary>
    public static async Task<Dictionary<string, int>> InsertOrIgnorePlexGenresVideoAsync(
        this IReaparrDbContext dbContext,
        IReadOnlyCollection<PlexGenreVideo> entities,
        CancellationToken ct
    )
    {
        var result = new Dictionary<string, int>(entities.Count);

        foreach (var chunk in entities.Chunk(CHUNK_SIZE))
        {
            var arguments = new List<object>(chunk.Length * 3);
            var values = new List<string>(chunk.Length);
            foreach (var genre in chunk)
            {
                values.Add($"({{{arguments.Count}}}, {{{arguments.Count + 1}}}, {{{arguments.Count + 2}}})");
                arguments.Add(genre.Name);
                arguments.Add(genre.Key);
                arguments.Add(JsonSerializer.Serialize(genre.Type, DefaultJsonSerializerOptions.ConfigStandard));
            }

            var sql = FormattableStringFactory.Create(
                $"INSERT OR IGNORE INTO PlexGenreVideo (Name, Key, Type) VALUES {string.Join(", ", values)}",
                arguments.ToArray()
            );
            await dbContext.ExecuteSqlInterpolatedAsync(sql, ct);

            var keys = chunk.Select(g => g.Key).ToList();
            var found = await dbContext
                .PlexGenresVideo.Where(g => keys.Contains(g.Key))
                .Select(g => new { g.Key, g.Id })
                .ToListAsync(ct);
            foreach (var genre in found)
                result[genre.Key] = genre.Id;
        }

        return result;
    }

    /// <summary>
    /// Inserts audio genres that are not already present and returns their database IDs keyed by Plex key.
    /// </summary>
    public static async Task<Dictionary<string, int>> InsertOrIgnorePlexGenresAudioAsync(
        this IReaparrDbContext dbContext,
        IReadOnlyCollection<PlexGenreAudio> entities,
        CancellationToken ct
    )
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
                $"INSERT OR IGNORE INTO PlexGenreAudio (Name, Key) VALUES {string.Join(", ", values)}",
                arguments.ToArray()
            );
            await dbContext.ExecuteSqlInterpolatedAsync(sql, ct);

            var keys = chunk.Select(g => g.Key).ToList();
            var found = await dbContext
                .PlexGenresAudio.Where(g => keys.Contains(g.Key))
                .Select(g => new { g.Key, g.Id })
                .ToListAsync(ct);
            foreach (var genre in found)
                result[genre.Key] = genre.Id;
        }

        return result;
    }

    /// <summary>
    /// Uses a single SQL statement to insert or ignore multiple <see cref="PlexCountry"/> entities into the database, and returns a dictionary mapping each country's key to its corresponding ID in the database.
    /// </summary>
    /// <param name="dbContext">The database context to use for the operation.</param>
    /// <param name="entities">A read-only collection of <see cref="PlexCountry"/> entities to insert or ignore.</param>
    /// <param name="ct">A cancellation token to observe while performing the operation.</param>
    /// <returns>A dictionary mapping each country's key to its corresponding ID in the database.</returns>
    public static async Task<Dictionary<string, int>> InsertOrIgnorePlexCountriesAsync(
        this IReaparrDbContext dbContext,
        IReadOnlyCollection<PlexCountry> entities,
        CancellationToken ct
    )
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
            await dbContext.ExecuteSqlInterpolatedAsync(sql, ct);

            var keys = chunk.Select(c => c.Key).ToList();
            var found = await dbContext
                .PlexCountries.Where(c => keys.Contains(c.Key))
                .Select(c => new { c.Key, c.Id })
                .ToListAsync(ct);
            foreach (var c in found)
                result[c.Key] = c.Id;
        }

        return result;
    }
}
