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

public class RefreshLibraryMediaCommandValidator : AbstractValidator<RefreshLibraryMediaCommand>
{
    public RefreshLibraryMediaCommandValidator()
    {
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
    }
}

public class RefreshLibraryMediaCommandHandler : IRequestHandler<RefreshLibraryMediaCommand, Result<PlexLibrary>>
{
    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ISignalRService _signalRService;
    private readonly IPlexApiService _plexServiceApi;

    private readonly int _baseCountProgress = 1000;
    private int _totalProgressSteps;

    private int _plexLibraryId;
    private Action<LibraryProgress>? _progressAction;

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

        _plexLibraryId = plexLibrary.Id;
        _progressAction = command.ProgressAction;

        _totalProgressSteps = plexLibrary.Type switch
        {
            PlexMediaType.TvShow => 5,
            PlexMediaType.Movie => 3,
            _ => _totalProgressSteps,
        };

        // Phase 1: Retrieve overview of all media belonging to this PlexLibrary
        var syncLibraryMediaResult = await _plexServiceApi.GetLibraryMediaAsync(
            plexLibrary,
            progress => SendProgress(1, progress.Percentage, progress.TimeRemaining),
            cancellationToken
        );

        if (syncLibraryMediaResult.IsFailed)
            return syncLibraryMediaResult.ToResult().LogError();

        // Phase 2: Sync the metadata such as Country, Roles and Genres for the library
        await _mediator.Send(
            new SyncPlexLibraryMediaMetaDataCommand(syncLibraryMediaResult.Value, plexLibrary.Id),
            cancellationToken
        );

        if (syncLibraryMediaResult.IsFailed)
            return syncLibraryMediaResult.ToResult().LogError();

        var newPlexLibrary = syncLibraryMediaResult.Value.Library;

        switch (newPlexLibrary.Type)
        {
            case PlexMediaType.Movie:
                return await RefreshPlexMovieLibrary(newPlexLibrary);
            case PlexMediaType.TvShow:
                return await _mediator.Send(new RefreshPlexTvShowLibraryCommand(newPlexLibrary), cancellationToken);
            default:
                return Result
                    .Fail($"Library type {newPlexLibrary.Type} is currently not supported by PlexRipper")
                    .LogWarning();
        }
    }

    private async Task<Result<PlexLibrary>> RefreshPlexMovieLibrary(PlexLibrary plexLibrary)
    {
        if (plexLibrary.Movies.Any())
        {
            for (var i = 0; i < plexLibrary.Movies.Count; i++)
            {
                var plexMovie = plexLibrary.Movies[i];
                plexMovie.PlexLibraryId = plexLibrary.Id;
                plexMovie.PlexServerId = plexLibrary.PlexServerId;
                plexMovie.SortIndex = i + 1;
            }

            var createResult = await _mediator.Send(new SyncPlexMoviesCommand(plexLibrary.Movies));
            if (createResult.IsFailed)
            {
                SendProgress(_totalProgressSteps, 1);
                return createResult.ToResult().LogError();
            }
        }
        else
        {
            _log.Warning(
                "No Movies were found for library {PlexLibraryName} with id: {PlexLibraryId}",
                plexLibrary.Title,
                plexLibrary.Id
            );
        }

        // Phase 2 of 3: PlexLibrary media data was parsed successfully.
        SendProgress(2, 1);

        var mediaSize = plexLibrary.Movies.Sum(x => x.MediaSize);
        plexLibrary.SetMovieMetaData(plexLibrary.Movies.Count, mediaSize);

        if (plexLibrary.Movies.Any() && mediaSize == 0)
        {
            _log.Error(
                "No media size was found for library {PlexLibraryName} with id: {PlexLibraryId}",
                plexLibrary.Title,
                plexLibrary.Id
            );
        }

        // Mark the library as synced
        plexLibrary.SyncedAt = DateTime.UtcNow;

        await _dbContext.UpdatePlexLibraryById(plexLibrary);

        _log.Information(
            "Successfully refreshed library {PlexLibraryName} with id: {PlexLibraryId}",
            plexLibrary.Title,
            plexLibrary.Id
        );

        // Phase 3 of 3: Movies have been successfully updated in the database.
        SendProgress(_totalProgressSteps, 1);

        return Result.Ok(plexLibrary);
    }

    private void SendProgress(int step, decimal percentage, TimeSpan timeRemaining = default)
    {
        var countStep = _baseCountProgress / _totalProgressSteps;
        var index = countStep * step + countStep * percentage;

        var progress = new LibraryProgress()
        {
            TimeRemaining = timeRemaining,
            Id = _plexLibraryId,
            Step = step,
            Received = (int)Math.Floor(index),
            Total = _baseCountProgress,
            TotalSteps = _totalProgressSteps,
        };

        _progressAction?.Invoke(progress);

        _signalRService.SendLibraryProgressUpdateAsync(progress);
    }
}
