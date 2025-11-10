using Microsoft.EntityFrameworkCore;

namespace Reaparr.Data.Contracts;

public static partial class DbContextExtensions
{
    public static async Task<string> GetPlexLibraryNameById(
        this IReaparrDbContext dbContext,
        int plexLibraryId,
        CancellationToken cancellationToken = default
    )
    {
        var plexLibraryName = await dbContext
            .PlexLibraries.Where(x => x.Id == plexLibraryId)
            .Select(x => x.Title)
            .FirstOrDefaultAsync(cancellationToken);
        return plexLibraryName ?? "Library Name Not Found";
    }

    public static async Task<int> GetPlexServerIdFromPlexLibraryId(this IReaparrDbContext dbContext, int plexLibraryId)
    {
        return await dbContext
            .PlexLibraries.Where(x => x.Id == plexLibraryId)
            .Select(x => x.PlexServerId)
            .FirstOrDefaultAsync(CancellationToken.None);
    }

    public static async Task SetLibraryMetaData(
        this IReaparrDbContext dbContext,
        int plexLibraryId,
        int actorsCount,
        int genreCount,
        int countryCount
    )
    {
        await dbContext
            .PlexLibraries.Where(x => x.Id == plexLibraryId)
            .ExecuteUpdateAsync(p =>
                p.SetProperty(x => x.ActorsCount, actorsCount)
                    .SetProperty(x => x.GenresCount, genreCount)
                    .SetProperty(x => x.CountriesCount, countryCount)
            );
    }

    public static async Task SetMovieMediaMetrics(
        this IReaparrDbContext dbContext,
        int plexLibraryId,
        int movieCount,
        long mediaSize
    )
    {
        await dbContext
            .PlexLibraries.Where(x => x.Id == plexLibraryId)
            .ExecuteUpdateAsync(p =>
                p.SetProperty(x => x.SyncedAt, DateTime.UtcNow)
                    .SetProperty(x => x.MovieCount, movieCount)
                    .SetProperty(x => x.MediaSize, mediaSize)
            );
    }

    public static async Task SetTvShowMediaMetrics(
        this IReaparrDbContext dbContext,
        int plexLibraryId,
        int tvShowCount,
        int seasonCount,
        int episodeCount,
        long mediaSize
    )
    {
        await dbContext
            .PlexLibraries.Where(x => x.Id == plexLibraryId)
            .ExecuteUpdateAsync(p =>
                p.SetProperty(x => x.SyncedAt, DateTime.UtcNow)
                    .SetProperty(x => x.TvShowCount, tvShowCount)
                    .SetProperty(x => x.SeasonCount, seasonCount)
                    .SetProperty(x => x.EpisodeCount, episodeCount)
                    .SetProperty(x => x.MediaSize, mediaSize)
            );
    }
}
