using System.Diagnostics;
using Data.Contracts;

namespace PlexRipper.Application;

public class PlexLibraryService : IPlexLibraryService
{
    #region Fields

    private readonly IMediator _mediator;

    private readonly IPlexApiService _plexServiceApi;

    private readonly ISignalRService _signalRService;

    #endregion

    #region Constructors

    public PlexLibraryService(
        IMediator mediator,
        IPlexApiService plexServiceApi,
        ISignalRService signalRService)
    {
        _mediator = mediator;
        _plexServiceApi = plexServiceApi;
        _signalRService = signalRService;
    }

    #endregion

    #region Methods

    #region Private

    /// <summary>
    /// Retrieves all TvShow, season and episode data and stores it in the database.
    /// </summary>
    /// <param name="plexLibrary"></param>
    /// <param name="progressAction"></param>
    /// <returns></returns>
    private async Task<Result> RefreshPlexTvShowLibrary(PlexLibrary plexLibrary, Action<LibraryProgress> progressAction = null)
    {
        if (plexLibrary == null)
            return ResultExtensions.IsNull(nameof(plexLibrary)).LogError();

        if (plexLibrary.Type != PlexMediaType.TvShow)
            return Result.Fail("PlexLibrary is not of type TvShow").LogError();

        if (plexLibrary.TvShows.Count == 0)
        {
            return Result.Fail(
                    $"PlexLibrary {plexLibrary.Name} with id {plexLibrary.Id} does not contain any TvShows and thus cannot request the corresponding media")
                .LogError();
        }

        // Send progress
        void SendProgress(int index, int count)
        {
            progressAction?.Invoke(new LibraryProgress(plexLibrary.Id, index, count));
            _signalRService.SendLibraryProgressUpdate(plexLibrary.Id, index, count);
        }

        // Request seasons and episodes for every tv show
        SendProgress(0, 4);

        var timer = new Stopwatch();
        timer.Start();

        var rawSeasonDataResult = await _plexServiceApi.GetAllSeasonsAsync(plexLibrary);
        if (rawSeasonDataResult.IsFailed)
            return rawSeasonDataResult.ToResult();

        // Phase 1 of 4: Season data was retrieved successfully.
        SendProgress(1, 4);

        var rawEpisodesDataResult = await _plexServiceApi.GetAllEpisodesAsync(plexLibrary);
        if (rawEpisodesDataResult.IsFailed)
            return rawEpisodesDataResult.ToResult();

        // Phase 2 of 4: Episode data was retrieved successfully.
        SendProgress(2, 4);

        foreach (var plexTvShow in plexLibrary.TvShows)
        {
            plexTvShow.Seasons = rawSeasonDataResult.Value.FindAll(x => x.ParentKey == plexTvShow.Key);
            plexTvShow.ChildCount = plexTvShow.Seasons.Count;

            foreach (var plexTvShowSeason in plexTvShow.Seasons)
            {
                plexTvShowSeason.PlexLibraryId = plexLibrary.Id;
                plexTvShowSeason.PlexLibrary = plexLibrary;
                plexTvShowSeason.TvShow = plexTvShow;
                plexTvShowSeason.Episodes = rawEpisodesDataResult.Value.FindAll(x => x.ParentKey == plexTvShowSeason.Key);
                plexTvShowSeason.ChildCount = plexTvShowSeason.Episodes.Count;

                // Assume the season started on the year of the first episode
                if (plexTvShowSeason.Year == 0 && plexTvShowSeason.Episodes.Any())
                    plexTvShowSeason.Year = plexTvShowSeason.Episodes.First().Year;

                // Set libraryId in each episode
                plexTvShowSeason.Episodes.ForEach(x => x.PlexLibraryId = plexLibrary.Id);
                plexTvShowSeason.MediaSize = plexTvShowSeason.Episodes.Sum(x => x.MediaSize);
            }

            plexTvShow.MediaSize = plexTvShow.Seasons.Sum(x => x.MediaSize);
        }

        // Phase 3 of 4: PlexLibrary media data was parsed successfully.
        SendProgress(3, 4);
        Log.Debug($"Finished retrieving all media for library {plexLibrary.Title} in {timer.Elapsed.TotalSeconds}");
        timer.Restart();

        // Update the MetaData of this library
        var updateMetaDataResult = plexLibrary.UpdateMetaData();
        if (updateMetaDataResult.IsFailed)
            return updateMetaDataResult;

        var updateResult = await _mediator.Send(new UpdatePlexLibraryByIdCommand(plexLibrary));
        if (updateResult.IsFailed)
            return updateResult.ToResult();

        var createResult = await _mediator.Send(new CreateUpdateOrDeletePlexTvShowsCommand(plexLibrary));
        if (createResult.IsFailed)
            return createResult.ToResult();

        Log.Debug($"Finished updating all media in the database for library {plexLibrary.Title} in {timer.Elapsed.TotalSeconds}");

        // Phase 4 of 4: Database has been successfully updated with new library data.
        SendProgress(4, 4);
        return Result.Ok();
    }

    /// <summary>
    /// Refresh the <see cref="PlexLibrary"/>, by first deleting all (related) media and the re-adding the media again.
    /// </summary>
    /// <param name="plexLibrary">The <see cref="PlexLibrary"/> to refresh.</param>
    /// <param name="progressAction"></param>
    /// <returns></returns>
    private async Task<Result> RefreshPlexMovieLibrary(PlexLibrary plexLibrary, Action<LibraryProgress> progressAction = null)
    {
        if (plexLibrary == null)
            return ResultExtensions.IsNull(nameof(plexLibrary));

        // Send progress
        void SendProgress(int index, int count)
        {
            progressAction?.Invoke(new LibraryProgress(plexLibrary.Id, index, count));
            _signalRService.SendLibraryProgressUpdate(plexLibrary.Id, index, count);
        }

        SendProgress(0, 3);

        // Update the MetaData of this library
        var updateMetaDataResult = plexLibrary.UpdateMetaData();
        if (updateMetaDataResult.IsFailed)
            return updateMetaDataResult;

        SendProgress(1, 3);

        var updateResult = await _mediator.Send(new UpdatePlexLibraryByIdCommand(plexLibrary));
        if (updateResult.IsFailed)
            return updateResult.ToResult();

        SendProgress(2, 3);

        var createResult = await _mediator.Send(new CreateUpdateOrDeletePlexMoviesCommand(plexLibrary));
        if (createResult.IsFailed)
            return createResult;

        SendProgress(3, 3);
        return Result.Ok();
    }

    #endregion

    #region Public

    /// <inheritdoc/>
    public async Task<Result<PlexLibrary>> GetPlexLibraryAsync(int libraryId, bool topLevelMediaOnly = false)
    {
        var libraryDB = await _mediator.Send(new GetPlexLibraryByIdQuery(libraryId));

        if (libraryDB.IsFailed)
            return libraryDB;

        if (!libraryDB.Value.HasMedia)
        {
            Log.Information($"PlexLibrary with id {libraryId} has no media, forcing refresh from the PlexApi");

            var refreshResult = await RefreshLibraryMediaAsync(libraryId);
            if (refreshResult.IsFailed)
                return refreshResult.ToResult();
        }

        return await _mediator.Send(new GetPlexLibraryByIdQuery(libraryId, true, true, topLevelMediaOnly));
    }

    public async Task<Result<List<PlexLibrary>>> GetAllPlexLibrariesAsync()
    {
        return await _mediator.Send(new GetAllPlexLibrariesQuery());
    }

    /// <inheritdoc/>
    public async Task<Result<PlexServer>> GetPlexLibraryInServerAsync(int libraryId, bool topLevelMediaOnly = false)
    {
        var plexLibrary = await GetPlexLibraryAsync(libraryId, topLevelMediaOnly);
        if (plexLibrary.IsFailed)
            return plexLibrary.ToResult();

        var plexServer = plexLibrary.Value.PlexServer;
        plexServer.PlexLibraries.Clear();
        plexServer.PlexLibraries.Add(plexLibrary.Value);
        return Result.Ok(plexServer);
    }

    public async Task<Result> UpdateDefaultDestinationLibrary(int libraryId, int folderPathId)
    {
        return await _mediator.Send(new UpdatePlexLibraryDefaultDestinationByIdCommand(libraryId, folderPathId));
    }

    #region RefreshLibrary

    public async Task<Result> RetrieveAccessibleLibrariesForAllAccountsAsync(int plexServerId)
    {
        if (plexServerId <= 0)
            return ResultExtensions.IsInvalidId(nameof(plexServerId)).LogWarning();

        var accountsResult = await _mediator.Send(new GetPlexAccountsWithAccessByPlexServerIdQuery(plexServerId));
        if (accountsResult.IsFailed)
            return accountsResult.ToResult();

        foreach (var plexAccount in accountsResult.Value)
            await RetrieveAccessibleLibrariesAsync(plexAccount.Id, plexServerId);

        return Result.Ok();
    }

    /// <inheritdoc/>
    public async Task<Result> RetrieveAccessibleLibrariesAsync(int plexAccountId, int plexServerId)
    {
        if (plexServerId <= 0)
            return ResultExtensions.IsInvalidId(nameof(plexServerId), plexServerId).LogWarning();

        if (plexAccountId <= 0)
            return ResultExtensions.IsInvalidId(nameof(plexAccountId), plexAccountId).LogWarning();

        Log.Debug($"Retrieving accessible PlexLibraries for plexServer with id: {plexServerId} by using Plex account with id {plexAccountId}");

        var libraries = await _plexServiceApi.GetLibrarySectionsAsync(plexServerId, plexAccountId);
        if (libraries.IsFailed)
            return libraries.ToResult();

        if (!libraries.Value.Any())
        {
            var msg = $"{nameof(PlexServer)} with Id {plexServerId} returned no Plex libraries for Plex account with id {plexAccountId}";
            return Result.Fail(msg).LogWarning();
        }

        return await _mediator.Send(new AddOrUpdatePlexLibrariesCommand(plexAccountId, libraries.Value));
    }

    public async Task<Result> RetrieveAllAccessibleLibrariesAsync(int plexAccountId)
    {
        Log.Information($"Retrieving accessible Plex libraries for Plex account with id {plexAccountId}");
        var plexServersResult = await _mediator.Send(new GetAllPlexServersByPlexAccountIdQuery(plexAccountId));
        if (plexServersResult.IsFailed)
            return plexServersResult.ToResult().LogError();

        //var onlineServers = plexServersResult.Value.FindAll(x => x.)

        // Create connection check tasks for all connections
        var retrieveTasks = plexServersResult.Value
            .Select(async plexServer => await RetrieveAccessibleLibrariesAsync(plexAccountId, plexServer.Id));

        var tasksResult = await Task.WhenAll(retrieveTasks);
        return tasksResult.Merge();
    }


    /// <inheritdoc/>
    public async Task<Result<PlexLibrary>> RefreshLibraryMediaAsync(int plexLibraryId, Action<LibraryProgress> progressAction = null)
    {
        var plexLibraryResult = await _mediator.Send(new GetPlexLibraryByIdQuery(plexLibraryId, true));
        if (plexLibraryResult.IsFailed)
            return plexLibraryResult;

        var plexLibrary = plexLibraryResult.Value;

        if (plexLibrary.Type != PlexMediaType.Movie && plexLibrary.Type != PlexMediaType.TvShow)
        {
            // TODO Remove this if all media types are supported
            return Result.Fail($"Library type {plexLibrary.Type} is currently not supported by PlexRipper").LogWarning();
        }

        // Retrieve overview of all media belonging to this PlexLibrary
        var newPlexLibraryResult = await _plexServiceApi.GetLibraryMediaAsync(plexLibrary);
        if (newPlexLibraryResult.IsFailed)
            return newPlexLibraryResult;

        var newPlexLibrary = newPlexLibraryResult.Value;

        // Get the default folder path id for the destination
        newPlexLibrary.DefaultDestinationId = newPlexLibrary.Type.ToDefaultDestinationFolderId();

        switch (newPlexLibrary.Type)
        {
            case PlexMediaType.Movie:
                return await RefreshPlexMovieLibrary(newPlexLibrary, progressAction);
            case PlexMediaType.TvShow:
                return await RefreshPlexTvShowLibrary(newPlexLibrary, progressAction);
        }

        Log.Information($"Successfully refreshed library {newPlexLibrary.Title} with id: {newPlexLibrary.Id}");
        return Result.Ok(newPlexLibrary);
    }

    #endregion

    #endregion

    #endregion
}