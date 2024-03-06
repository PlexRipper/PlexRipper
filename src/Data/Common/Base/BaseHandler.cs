using Data.Contracts;
using EFCore.BulkExtensions;
using Logging.Interface;

namespace PlexRipper.Data.Common;

public abstract class BaseHandler
{
    #region Fields

    protected readonly ILog _log;

    private protected readonly PlexRipperDbContext _dbContext;

    private protected readonly BulkConfig _bulkConfig = new()
    {
        SetOutputIdentity = true,
        PreserveInsertOrder = true,
    };

    #endregion

    #region Constructors

    protected BaseHandler(ILog log, PlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    #endregion

    #region Properties

    protected IQueryable<PlexServer> PlexServerQueryable => _dbContext.PlexServers.AsQueryable();

    protected IQueryable<PlexLibrary> PlexLibraryQueryable => _dbContext.PlexLibraries.AsQueryable();

    /// <summary>
    /// Creates a PlexMovie IQueryable.
    /// </summary>
    protected IQueryable<PlexMovie> PlexMoviesQueryable => _dbContext.PlexMovies.AsQueryable();

    /// <summary>
    /// Creates a PlexTvShow IQueryable.
    /// </summary>
    protected IQueryable<PlexTvShow> PlexTvShowsQueryable => _dbContext.PlexTvShows.AsQueryable();

    /// <summary>
    /// Creates a PlexTvShowSeason IQueryable.
    /// </summary>
    protected IQueryable<PlexTvShowSeason> PlexTvShowSeasonsQueryable => _dbContext.PlexTvShowSeason.AsQueryable();

    /// <summary>
    /// Creates a PlexTvShowEpisode IQueryable.
    /// </summary>
    protected IQueryable<PlexTvShowEpisode> PlexTvShowEpisodesQueryable => _dbContext.PlexTvShowEpisodes.AsQueryable();

    #endregion

    #region Methods

    /// <summary>
    /// Only include <see cref="PlexLibrary"/> related data based on the <see cref="PlexMediaType"/>.
    /// </summary>
    /// <param name="type">The <see cref="PlexMediaType"/> to use to include the related data.</param>
    /// <param name="includeServer">Optionally include the <see cref="PlexServer"/> this <see cref="PlexLibrary"/> belongs to.</param>
    /// <param name="includeMedia"></param>
    /// <param name="topLevelMediaOnly"></param>
    /// <returns>The <see cref="IQueryable"/> of <see cref="PlexLibrary"/>.</returns>
    protected IQueryable<PlexLibrary> GetPlexLibraryQueryableByType(
        PlexMediaType type,
        bool includeServer = false,
        bool includeMedia = false,
        bool topLevelMediaOnly = false)
    {
        var plexLibraryQuery = PlexLibraryQueryable;

        if (includeServer)
            plexLibraryQuery = plexLibraryQuery.IncludePlexServer();

        if (includeMedia)
        {
            switch (type)
            {
                case PlexMediaType.Movie:
                    return plexLibraryQuery.IncludeMovies();
                case PlexMediaType.TvShow:
                    return plexLibraryQuery.IncludeTvShows(topLevelMediaOnly);
                default:
                    _log.Error("PlexLibrary with MediaType {Type} is currently not supported", type);
                    return plexLibraryQuery;
            }
        }

        return plexLibraryQuery;
    }

    protected Result<T> ReturnResult<T>(T value, int id = 0)
        where T : BaseEntity
    {
        if (value != null) return Result.Ok(value);

        return Result.Fail(new Error($"Could not find an entity of {typeof(T)} with an id of {id}"));
    }

    protected Result<List<T>> ReturnResult<T>(List<T> value, int id = 0)
    {
        if (value != null && value.Any()) return Result.Ok(value);

        return Result.Fail(new Error($"Could not find entities of {typeof(T)} with an id of {id}"));
    }

    protected async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    #endregion

    // public void Dispose()
    // {
    //     // This is here as a reminder that the dbContext will be disposed by the dependency injection container
    // }
}