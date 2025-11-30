using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.BackgroundJobs.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.BackgroundJobs;

public record QueueNextPlexLibraryToSyncCommand : ICommand<Result>
{
    public int PlexServerId { get; set; }
}

public class QueueNextPlexLibraryToSyncCommandValidator : AbstractValidator<QueueNextPlexLibraryToSyncCommand>
{
    public QueueNextPlexLibraryToSyncCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class QueueNextPlexLibraryToSyncCommandHandler : ICommandHandler<QueueNextPlexLibraryToSyncCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public QueueNextPlexLibraryToSyncCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
 
    )
    {
        _log = log.ForContext<QueueNextPlexLibraryToSyncCommandHandler>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public async Task<Result> ExecuteAsync(
        QueueNextPlexLibraryToSyncCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexServerId = command.PlexServerId;
        var plexServerName = await _dbContext.GetPlexServerNameById(plexServerId, cancellationToken: cancellationToken);

        var plexLibraries = await _dbContext
            .PlexLibraries.Where(x => x.PlexServerId == plexServerId && (x.Outdated || x.SyncedAt == null))
            .ToListAsync(cancellationToken: cancellationToken);

        if (!plexLibraries.Any())
        {
            _log.Here()
                .Debug(
                    "No outdated Plex libraries found for Plex server {PlexServerName} with id {PlexServerId}",
                    plexServerName,
                    plexServerId
                );
            return Result.Ok();
        }

        var movieLibraries = plexLibraries.Where(x => x.Type == PlexMediaType.Movie).ToList();
        var tvShowLibraries = plexLibraries.Where(x => x.Type == PlexMediaType.TvShow).ToList();

        if (!movieLibraries.Any() && !tvShowLibraries.Any())
        {
            _log.Here()
                .Debug(
                    "No movie or TV show libraries found to sync for Plex server {PlexServerName} with id {PlexServerId}",
                    plexServerName,
                    plexServerId
                );
            return Result.Ok();
        }

        // Prioritize movie libraries first since they are usually smaller and faster to sync
        if (movieLibraries.Any())
        {
            var nextLibraryId = movieLibraries.First().Id;
            await _commandExecutor.Send(new ScheduleLibrarySyncCommand(plexServerId, nextLibraryId), cancellationToken);
            return Result.Ok();
        }

        if (tvShowLibraries.Any())
        {
            var nextLibraryId = tvShowLibraries.First().Id;
            await _commandExecutor.Send(new ScheduleLibrarySyncCommand(plexServerId, nextLibraryId), cancellationToken);
        }

        return Result.Ok();
    }
}