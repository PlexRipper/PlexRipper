using Application.Contracts;
using Data.Contracts;
using Logging.Interface;
using PlexApi.Contracts;

namespace PlexRipper.Application;

public class PlexLibraryService : IPlexLibraryService
{
    #region Fields

    private readonly ILog _log;
    private readonly IMediator _mediator;

    private readonly IPlexApiService _plexServiceApi;
    private readonly IPlexRipperDbContext _dbContext;

    #endregion

    #region Constructors

    public PlexLibraryService(
        ILog log,
        IMediator mediator,
        IPlexApiService plexServiceApi,
        IPlexRipperDbContext dbContext)
    {
        _log = log;
        _mediator = mediator;
        _plexServiceApi = plexServiceApi;
        _dbContext = dbContext;
    }

    #endregion

    #region Methods

    #region Public

    /// <inheritdoc/>
    public async Task<Result<PlexLibrary>> GetPlexLibraryAsync(int libraryId)
    {
        var libraryDB = await _mediator.Send(new GetPlexLibraryByIdQuery(libraryId));

        if (libraryDB.IsFailed)
            return libraryDB;

        if (!libraryDB.Value.HasMedia)
        {
            _log.Information("PlexLibrary with id {LibraryId} has no media, forcing refresh from the PlexApi", libraryId);

            var refreshResult = await _mediator.Send(new RefreshLibraryMediaCommand(libraryId));
            if (refreshResult.IsFailed)
                return refreshResult.ToResult();
        }

        return await _mediator.Send(new GetPlexLibraryByIdQuery(libraryId));
    }

    #endregion

    #endregion

    #region RefreshLibrary



    #endregion
}