using FastEndpoints;
using FluentValidation;

namespace Reaparr.BackgroundJobs;

/// <summary>
/// Command to move a library sync to the front of the queue by canceling the current execution and rescheduling with the highest priority.
/// </summary>
public record MoveLibrarySyncToFrontCommand(int PlexServerId, int LibraryId) : ICommand<Result>;

public class MoveLibrarySyncToFrontCommandValidator : AbstractValidator<MoveLibrarySyncToFrontCommand>
{
    public MoveLibrarySyncToFrontCommandValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
        RuleFor(x => x.LibraryId).GreaterThan(0);
    }
}

public class MoveLibrarySyncToFrontCommandHandler : ICommandHandler<MoveLibrarySyncToFrontCommand, Result>
{
    private readonly ILogger _log;

    public MoveLibrarySyncToFrontCommandHandler(ILogger log)
    {
        _log = log.ForContext<MoveLibrarySyncToFrontCommandHandler>();
    }

    public async Task<Result> ExecuteAsync(MoveLibrarySyncToFrontCommand command, CancellationToken ct)
    {
        _log.Here()
            .Information(
                "Moving library sync to front for server {PlexServerId}, library {LibraryId}",
                command.PlexServerId,
                command.LibraryId
            );

        await Task.CompletedTask;
        throw new NotImplementedException();

        return Result.Ok();
    }
}
