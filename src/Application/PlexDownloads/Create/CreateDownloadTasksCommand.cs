using DownloadManager.Contracts;
using DownloadManager.Contracts.Extensions;
using FluentValidation;

namespace PlexRipper.Application;

/// <summary>
/// Generates a nested list of <see cref="DownloadTaskGeneric"/> and adds to the download queue.
/// </summary>
/// <returns>Returns true if all downloadTasks were added successfully.</returns>
public record CreateDownloadTasksCommand(List<DownloadMediaDTO> DownloadMediaDtos) : IRequest<Result>;

public class CreateDownloadTasksCommandValidator : AbstractValidator<CreateDownloadTasksCommand>
{
    public CreateDownloadTasksCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x).NotEmpty();
    }
}

public class CreateDownloadTasksCommandHandler : IRequestHandler<CreateDownloadTasksCommand, Result>
{
    private readonly IMediator _mediator;

    public CreateDownloadTasksCommandHandler(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result> Handle(CreateDownloadTasksCommand command, CancellationToken cancellationToken)
    {
        if (command.DownloadMediaDtos.Any(x => x.Type == PlexMediaType.Movie))
        {
            var result = await _mediator.Send(new GenerateDownloadTaskMoviesCommand(command.DownloadMediaDtos), cancellationToken);
            result.LogIfFailed();
        }

        if (command.DownloadMediaDtos.Any(x => x.Type == PlexMediaType.TvShow))
        {
            var result = await _mediator.Send(new GenerateDownloadTaskTvShowsCommand(command.DownloadMediaDtos), cancellationToken);
            result.LogIfFailed();
        }

        if (command.DownloadMediaDtos.Any(x => x.Type == PlexMediaType.Season))
        {
            var result = await _mediator.Send(new GenerateDownloadTaskTvShowSeasonsCommand(command.DownloadMediaDtos), cancellationToken);
            result.LogIfFailed();
        }

        if (command.DownloadMediaDtos.Any(x => x.Type == PlexMediaType.Episode))
        {
            var result = await _mediator.Send(new GenerateDownloadTaskTvShowEpisodesCommand(command.DownloadMediaDtos), cancellationToken);
            result.LogIfFailed();
        }

        // Notify the DownloadQueue to check for new tasks in the PlexSevers with new DownloadTasks
        var uniquePlexServers = command.DownloadMediaDtos
            .MergeAndGroupList()
            .Select(x => x.PlexServerId)
            .Distinct()
            .ToList();
        await _mediator.Publish(new CheckDownloadQueue(uniquePlexServers), cancellationToken);
        return Result.Ok();
    }
}