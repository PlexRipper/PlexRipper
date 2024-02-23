using Data.Contracts;
using DownloadManager.Contracts;
using EFCore.BulkExtensions;
using FluentValidation;
using Logging.Interface;

namespace PlexRipper.Application;

/// <summary>
/// Generates a nested list of <see cref="DownloadTask"/> and adds to the download queue.
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
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IDownloadTaskFactory _downloadTaskFactory;
    private readonly IDownloadTaskValidator _downloadTaskValidator;

    private protected readonly BulkConfig _bulkConfig = new()
    {
        SetOutputIdentity = true,
        PreserveInsertOrder = true,
    };

    public CreateDownloadTasksCommandHandler(
        ILog log,
        IPlexRipperDbContext dbContext,
        IMediator mediator,
        IDownloadTaskFactory downloadTaskFactory,
        IDownloadTaskValidator downloadTaskValidator)
    {
        _log = log;
        _dbContext = dbContext;
        _mediator = mediator;
        _downloadTaskFactory = downloadTaskFactory;
        _downloadTaskValidator = downloadTaskValidator;
    }

    public async Task<Result> Handle(CreateDownloadTasksCommand command, CancellationToken cancellationToken)
    {
        var downloadTasks = await _downloadTaskFactory.GenerateAsync(command.DownloadMediaDtos);
        if (downloadTasks.IsFailed)
            return downloadTasks.ToResult();

        var validateResult = _downloadTaskValidator.ValidateDownloadTasks(downloadTasks.Value);
        if (validateResult.IsFailed)
            return validateResult.ToResult().LogDebug();

        // Add to Database
        await InsertDownloadTasks(validateResult.Value, cancellationToken);

        _log.Debug("Successfully added all {ValidateCount} DownloadTasks", validateResult.Value.Flatten(x => x.Children).ToList().Count);

        // Notify the DownloadQueue to check for new tasks in the PlexSevers with new DownloadTasks
        var uniquePlexServers = downloadTasks.Value.Select(x => x.PlexServerId).Distinct().ToList();
        await _mediator.Publish(new CheckDownloadQueue(uniquePlexServers), cancellationToken);
        return Result.Ok();
    }

    private async Task<Result> InsertDownloadTasks(List<DownloadTask> downloadTasks, CancellationToken cancellationToken = default)
    {
        // Prevent the navigation properties from being updated
        downloadTasks.ForEach(x =>
        {
            x.DestinationFolder = null;
            x.DownloadFolder = null;
            x.PlexServer = null;
            x.PlexLibrary = null;
        });

        // Only create new tasks, downloadTasks can be nested in tasks that already are in the database.
        await _dbContext.BulkInsertAsync(downloadTasks.FindAll(x => x.Id == 0), _bulkConfig, cancellationToken);

        foreach (var downloadTask in downloadTasks)
        {
            if (downloadTask.Children is null || !downloadTask.Children.Any())
                continue;

            downloadTask.Children = downloadTask.Children.SetRootId(downloadTask.Id);
        }

        InsertChildDownloadTasks(downloadTasks);

        return Result.Ok();
    }

    private void InsertChildDownloadTasks(List<DownloadTask> downloadTasks)
    {
        downloadTasks.ForEach(x =>
        {
            x.DestinationFolder = null;
            x.DownloadFolder = null;
            x.PlexServer = null;
            x.PlexLibrary = null;
        });

        // Only create new tasks, downloadTasks can be nested in tasks that already are in the database.
        _dbContext.BulkInsertAsync(downloadTasks.FindAll(x => x.Id == 0), _bulkConfig);

        foreach (var downloadTask in downloadTasks)
            if (downloadTask.Children.Any())
            {
                foreach (var downloadTaskChild in downloadTask.Children)
                {
                    downloadTaskChild.ParentId = downloadTask.Id;
                    downloadTaskChild.Parent = null;
                }

                InsertChildDownloadTasks(downloadTask.Children);
            }
    }
}