using Application.Contracts;
using FluentValidation;

namespace PlexRipper.Application;

/// <summary>
/// Generates a nested list of <see cref="DownloadTaskGeneric"/> and adds to the download queue.
/// </summary>
/// <returns>Returns true if all downloadTasks were added successfully.</returns>
public record CreateDownloadTasksCommand : IRequest<Result>
{
    public CreateDownloadTasksCommand(CreateDownloadTasksRequest request)
    {
        Request = request;
    }

    public CreateDownloadTasksCommand(List<DownloadMediaDTO> downloadMediaDtos)
    {
        Request = new CreateDownloadTasksRequest(downloadMediaDtos);
    }

    public CreateDownloadTasksRequest Request { get; }
}

public class CreateDownloadTasksCommandValidator : AbstractValidator<CreateDownloadTasksCommand>
{
    public CreateDownloadTasksCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.Request.DownloadMedias).NotEmpty();
    }
}

public class CreateDownloadTasksCommandHandler : IRequestHandler<CreateDownloadTasksCommand, Result>
{
    private readonly IMediator _mediator;
    private bool _generatedTasks;

    public CreateDownloadTasksCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result> Handle(CreateDownloadTasksCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var downloadMedias = command.Request.DownloadMedias;

        if (downloadMedias.Any(x => x.Type == PlexMediaType.Movie))
        {
            var result = await _mediator.Send(new GenerateDownloadTaskMoviesCommand(request), cancellationToken);
            result.LogIfFailed();
            _generatedTasks = true;
        }

        if (downloadMedias.Any(x => x.Type == PlexMediaType.TvShow))
        {
            var result = await _mediator.Send(new GenerateDownloadTaskTvShowsCommand(request), cancellationToken);
            result.LogIfFailed();
            _generatedTasks = true;
        }

        if (downloadMedias.Any(x => x.Type == PlexMediaType.Season))
        {
            var result = await _mediator.Send(new GenerateDownloadTaskTvShowSeasonsCommand(request), cancellationToken);
            result.LogIfFailed();
            _generatedTasks = true;
        }

        if (downloadMedias.Any(x => x.Type == PlexMediaType.Episode))
        {
            var result = await _mediator.Send(
                new GenerateDownloadTaskTvShowEpisodesCommand(request),
                cancellationToken
            );
            result.LogIfFailed();
            _generatedTasks = true;
        }

        if (_generatedTasks)
        {
            // Notify the DownloadQueue to check for new tasks in the PlexSevers with new DownloadTasks
            var uniquePlexServers = request
                .DownloadMedias.MergeAndGroupList()
                .Select(x => x.PlexServerId)
                .Distinct()
                .ToList();

            await _mediator.Publish(new CheckDownloadQueueNotification(uniquePlexServers), cancellationToken);
        }

        return Result.Ok();
    }
}
