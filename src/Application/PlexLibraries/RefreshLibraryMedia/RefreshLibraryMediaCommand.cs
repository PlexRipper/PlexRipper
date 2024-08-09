using System.Diagnostics;
using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexApi.Contracts;
using WebAPI.Contracts;

namespace PlexRipper.Application;

/// <summary>
/// Retrieves the new media metadata from the PlexApi and stores it in the database.
/// </summary>
/// <param name="PlexLibraryId">The id of the <see cref="PlexLibrary"/> to retrieve.</param>
/// <param name="ProgressAction">The action to call for a progress update.</param>
/// <returns>Returns the PlexLibrary with the containing media.</returns>
public record RefreshLibraryMediaCommand(int PlexLibraryId, Action<LibraryProgress>? ProgressAction = null)
    : IRequest<Result<PlexLibrary>>;

public class RefreshLibraryMediaCommandValidator : AbstractValidator<RefreshLibraryMediaCommand> { }

public class RefreshLibraryMediaCommandHandler : IRequestHandler<RefreshLibraryMediaCommand, Result<PlexLibrary>>
{
    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ISignalRService _signalRService;
    private readonly IPlexApiService _plexServiceApi;

    public RefreshLibraryMediaCommandHandler(
        ILog log,
        IMediator mediator,
        IPlexRipperDbContext dbContext,
        ISignalRService signalRService,
        IPlexApiService plexServiceApi
    )
    {
        _log = log;
        _mediator = mediator;
        _dbContext = dbContext;
        _signalRService = signalRService;
        _plexServiceApi = plexServiceApi;
    }

    public async Task<Result<PlexLibrary>> Handle(
        RefreshLibraryMediaCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexLibrary = await _dbContext
            .PlexLibraries.Include(x => x.PlexServer)
            .FirstOrDefaultAsync(x => x.Id == command.PlexLibraryId, cancellationToken);

        if (plexLibrary is null)
            return ResultExtensions.EntityNotFound(nameof(plexLibrary), command.PlexLibraryId);

        // Retrieve overview of all media belonging to this PlexLibrary
        var newPlexLibraryResult = await _plexServiceApi.GetLibraryMediaAsync(plexLibrary, cancellationToken);
        if (newPlexLibraryResult.IsFailed)
            return newPlexLibraryResult;

        var newPlexLibrary = newPlexLibraryResult.Value;

        // Get the default folder path id for the destination
        newPlexLibrary.DefaultDestinationId = newPlexLibrary.Type.ToDefaultDestinationFolderId();

        switch (newPlexLibrary.Type)
        {
            case PlexMediaType.Movie:
                return await RefreshPlexMovieLibrary(newPlexLibrary, command.ProgressAction);
            case PlexMediaType.TvShow:
                return await RefreshPlexTvShowLibrary(newPlexLibrary, command.ProgressAction);
            default:
                return Result
                    .Fail($"Library type {newPlexLibrary.Type} is currently not supported by PlexRipper")
                    .LogWarning();
        }
    }

    private async Task<Result<PlexLibrary>> RefreshPlexTvShowLibrary(
        PlexLibrary plexLibrary,
        Action<LibraryProgress>? progressAction = null
    )
    {
        if (plexLibrary.Type != PlexMediaType.TvShow)
            return Result.Fail("PlexLibrary is not of type TvShow").LogError();

        if (plexLibrary.TvShows.Count == 0)
        {
            return Result
                .Fail(
                    $"PlexLibrary {plexLibrary.Name} with id {plexLibrary.Id} does not contain any TvShows and thus cannot request the corresponding media"
                )
                .LogError();
        }

        // Send progress
        void SendProgress(int index, int count)
        {
            progressAction?.Invoke(new LibraryProgress(plexLibrary.Id, index, count));

            _signalRService.SendLibraryProgressUpdateAsync(plexLibrary.Id, index, count);
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
                plexTvShowSeason.Episodes = rawEpisodesDataResult.Value.FindAll(x =>
                    x.ParentKey == plexTvShowSeason.Key
                );
                plexTvShowSeason.ChildCount = plexTvShowSeason.Episodes.Count;

                // Assume the season started on the year of the first episode
                if (plexTvShowSeason.Year == 0 && plexTvShowSeason.Episodes.Any())
                    plexTvShowSeason.Year = plexTvShowSeason.Episodes.First().Year;

                // Set libraryId in each episode
                plexTvShowSeason.Episodes.ForEach(x => x.PlexLibraryId = plexLibrary.Id);
                plexTvShowSeason.MediaSize = plexTvShowSeason.Episodes.Sum(x => x.MediaSize);
                plexTvShowSeason.Duration = plexTvShowSeason.Episodes.Sum(x => x.Duration);
            }

            plexTvShow.MediaSize = plexTvShow.Seasons.Sum(x => x.MediaSize);
            plexTvShow.Duration = plexTvShow.Seasons.Sum(x => x.Duration);
        }

        // Phase 3 of 4: PlexLibrary media data was parsed successfully.
        SendProgress(3, 4);
        _log.Here()
            .Debug(
                "Finished retrieving all media for library {PlexLibraryName} in {Elapsed:000} seconds",
                plexLibrary.Title,
                timer.Elapsed.TotalSeconds
            );
        timer.Restart();

        // Update the MetaData of this library
        var updateMetaDataResult = plexLibrary.UpdateMetaData();
        if (updateMetaDataResult.IsFailed)
            return updateMetaDataResult;

        await _dbContext.UpdatePlexLibraryById(plexLibrary);

        var createResult = await _mediator.Send(new CreateUpdateOrDeletePlexTvShowsCommand(plexLibrary));
        if (createResult.IsFailed)
            return createResult.ToResult();

        _log.Here()
            .Debug(
                "Finished updating all media in the database for library {PlexLibraryName} in {Elapsed:000} seconds",
                plexLibrary.Title,
                timer.Elapsed.TotalSeconds
            );

        // Phase 4 of 4: Database has been successfully updated with new library data.
        SendProgress(4, 4);

        _log.Information(
            "Successfully refreshed library {PlexLibraryName} with id: {PlexLibraryId}",
            plexLibrary.Title,
            plexLibrary.Id
        );

        // Mark the library as synced
        plexLibrary.SyncedAt = DateTime.UtcNow;
        await _dbContext
            .PlexLibraries.Where(x => x.Id == plexLibrary.Id)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.SyncedAt, plexLibrary.SyncedAt));

        return Result.Ok(plexLibrary);
    }

    private async Task<Result<PlexLibrary>> RefreshPlexMovieLibrary(
        PlexLibrary plexLibrary,
        Action<LibraryProgress>? progressAction = null
    )
    {
        // Send progress
        void SendProgress(int index, int count)
        {
            progressAction?.Invoke(new LibraryProgress(plexLibrary.Id, index, count));
            _signalRService.SendLibraryProgressUpdateAsync(plexLibrary.Id, index, count);
        }

        SendProgress(0, 3);

        // Update the MetaData of this library
        var updateMetaDataResult = plexLibrary.UpdateMetaData();
        if (updateMetaDataResult.IsFailed)
            return updateMetaDataResult;

        SendProgress(1, 3);

        await _dbContext.UpdatePlexLibraryById(plexLibrary);

        SendProgress(2, 3);

        var createResult = await _mediator.Send(new CreateUpdateOrDeletePlexMoviesCommand(plexLibrary));
        if (createResult.IsFailed)
            return createResult;

        SendProgress(3, 3);

        _log.Information(
            "Successfully refreshed library {PlexLibraryName} with id: {PlexLibraryId}",
            plexLibrary.Title,
            plexLibrary.Id
        );

        // Mark the library as synced
        plexLibrary.SyncedAt = DateTime.UtcNow;
        await _dbContext
            .PlexLibraries.Where(x => x.Id == plexLibrary.Id)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.SyncedAt, plexLibrary.SyncedAt));

        return Result.Ok(plexLibrary);
    }
}
