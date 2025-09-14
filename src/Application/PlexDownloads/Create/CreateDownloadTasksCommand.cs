using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;

namespace Reaparr.Application;

public class CreateDownloadTasksCommandValidator : AbstractValidator<CreateDownloadTasksCommand>
{
    public CreateDownloadTasksCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.Request.DownloadMedias).NotEmpty();
    }
}

public class CreateDownloadTasksCommandHandler : ICommandHandler<CreateDownloadTasksCommand, Result>
{
    private readonly ICommandExecutor _commandExecutor;
    private readonly IEventPublisher _eventPublisher;
    private bool _generatedTasks;

    public CreateDownloadTasksCommandHandler(ICommandExecutor commandExecutor, IEventPublisher eventPublisher)
    {
        _commandExecutor = commandExecutor;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result> ExecuteAsync(CreateDownloadTasksCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var downloadMedias = command.Request.DownloadMedias;

        if (downloadMedias.Any(x => x.Type == PlexMediaType.Movie))
        {
            var result = await _commandExecutor.Send(new GenerateDownloadTaskMoviesCommand(request), cancellationToken);
            result.LogIfFailed();
            _generatedTasks = true;
        }

        if (downloadMedias.Any(x => x.Type == PlexMediaType.TvShow))
        {
            var result = await _commandExecutor.Send(
                new GenerateDownloadTaskTvShowsCommand(request),
                cancellationToken
            );
            result.LogIfFailed();
            _generatedTasks = true;
        }

        if (downloadMedias.Any(x => x.Type == PlexMediaType.Season))
        {
            var result = await _commandExecutor.Send(
                new GenerateDownloadTaskTvShowSeasonsCommand(request),
                cancellationToken
            );
            result.LogIfFailed();
            _generatedTasks = true;
        }

        if (downloadMedias.Any(x => x.Type == PlexMediaType.Episode))
        {
            var result = await _commandExecutor.Send(
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

            await _eventPublisher.PublishAsync(new CheckDownloadQueueEvent(uniquePlexServers), cancellationToken);
        }

        return Result.Ok();
    }
}
