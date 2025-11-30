using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

/// <summary>
/// Command to sync all libraries for a Plex server by scheduling the first library sync job.
/// The job chain will handle the rest automatically.
/// </summary>
public record SyncServerMediaJobCommand(int PlexServerId, bool ForceSync = false) : ICommand<Result>;

public class SyncServerMediaJobCommandValidator : AbstractValidator<SyncServerMediaJobCommand>
{
    public SyncServerMediaJobCommandValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class SyncServerMediaJobCommandHandler : ICommandHandler<SyncServerMediaJobCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ISignalRService _signalRService;

    public SyncServerMediaJobCommandHandler(ILogger log, IReaparrDbContext dbContext, ISignalRService signalRService)
    {
        _log = log.ForContext<SyncServerMediaJobCommandHandler>();
        _dbContext = dbContext;
        _signalRService = signalRService;
    }

    public async Task<Result> ExecuteAsync(SyncServerMediaJobCommand command, CancellationToken ct)
    {
        var plexServer = await _dbContext.PlexServers.IncludeLibraries().GetAsync(command.PlexServerId, ct);
        if (plexServer is null)
        {
            return ResultExtensions.EntityNotFound(nameof(PlexServer), command.PlexServerId).LogError();
        }

        if (!plexServer.IsEnabled)
        {
            var plexServerName = await _dbContext.GetPlexServerNameById(command.PlexServerId, ct);
            return ResultExtensions
                .ServerIsNotEnabled(plexServerName, command.PlexServerId, nameof(SyncServerMediaJobCommand))
                .LogError();
        }

        var plexLibraries = command.ForceSync
            ? plexServer.PlexLibraries
            : plexServer
                .PlexLibraries.Where(x => x is { Outdated: true, Type: PlexMediaType.Movie or PlexMediaType.TvShow })
                .ToList();

        if (!plexLibraries.Any())
        {
            _log.Here()
                .Information(
                    "PlexServer {PlexServerName} with id {PlexServerId} has no libraries to sync",
                    plexServer.Name,
                    plexServer.Id
                );
            return Result.Ok();
        }

        // Order libraries: movies first, then TV shows
        var orderedLibraries = plexLibraries
            .Where(x => x.Type == PlexMediaType.Movie)
            .Concat(plexLibraries.Where(x => x.Type == PlexMediaType.TvShow))
            .ToList();

        var libraryIds = orderedLibraries.Select(x => x.Id).ToList();

        if (!libraryIds.Any())
        {
            _log.Here()
                .Information(
                    "PlexServer {PlexServerName} with id {PlexServerId} has no libraries to sync after filtering",
                    plexServer.Name,
                    plexServer.Id
                );
            return Result.Ok();
        }

        // Schedule the first library with the remaining library IDs
        var firstLibraryId = libraryIds[0];
        var remainingLibraryIds = libraryIds.Skip(1).ToList();

        _log.Here()
            .Information(
                "Scheduling library sync chain for server {PlexServerName} (id: {PlexServerId}) starting with library {FirstLibraryId} ({RemainingCount} remaining)",
                plexServer.Name,
                command.PlexServerId,
                firstLibraryId,
                remainingLibraryIds.Count
            );

        await _librarySyncScheduler.ScheduleLibrary(command.PlexServerId, firstLibraryId, remainingLibraryIds, ct);

        // Send refresh notification
        await _signalRService.SendRefreshNotificationAsync(RefreshDataType.PlexLibrary, ct);

        return Result.Ok();
    }
}
