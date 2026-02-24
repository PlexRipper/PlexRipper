using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.PublicAPI;

public sealed class DeleteTorrentRequest
{
    [FromBody]
    public List<string>? Hashes { get; init; }

    [FormField, BindFrom("hashes")]
    public string? HashesRaw { get; init; }

    [FormField, BindFrom("deleteFiles")]
    public bool? DeleteFiles { get; init; }
}

public sealed class DeleteTorrentRequestValidator : Validator<DeleteTorrentRequest>
{
    public DeleteTorrentRequestValidator()
    {
        RuleForEach(x => x.Hashes)
            .Must(hash => !string.IsNullOrWhiteSpace(hash))
            .When(x => x.Hashes is { Count: > 0 })
            .WithMessage("Hashes must not be empty.");

        RuleFor(x => x.HashesRaw)
            .Must(raw =>
                string.IsNullOrWhiteSpace(raw)
                || string.Equals(raw, "all", StringComparison.OrdinalIgnoreCase)
                || raw.Split(
                        ['|', ',', ';', ' ', '\t', '\r', '\n'],
                        StringSplitOptions.RemoveEmptyEntries
                    )
                    .Length
                    > 0
            )
            .WithMessage("Hashes must be 'all' or a delimited list of hashes.");
    }
}

public sealed class DeleteTorrentEndpoint : Endpoint<DeleteTorrentRequest>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    private sealed record DownloadTaskLookup(Guid Id, DownloadStatus Status);

    public DeleteTorrentEndpoint(ILogger log, IReaparrDbContext dbContext, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<DeleteTorrentEndpoint>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Post(PublicApiRoutes.DownloadClient + "/torrents/delete");
        Description(x => x.IsDownloadClient());
        AllowFormData(urlEncoded: true);
        AllowAnonymous();
        PreProcessor<DownloadClientAuthenticationPreProcessor<DeleteTorrentRequest>>();
    }

    public override async Task HandleAsync(DeleteTorrentRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        var deleteFiles = req.DeleteFiles ?? true;

        var hashes = new List<string>();
        if (req.Hashes?.Count > 0)
            hashes.AddRange(req.Hashes);

        if (!string.IsNullOrWhiteSpace(req.HashesRaw))
        {
            if (string.Equals(req.HashesRaw, "all", StringComparison.OrdinalIgnoreCase))
            {
                await ClearCompleted([], ct);
                await Send.StringAsync("Ok.", cancellation: ct);
                return;
            }

            var split = req.HashesRaw.Split(
                ['|', ',', ';', ' ', '\t', '\r', '\n'],
                StringSplitOptions.RemoveEmptyEntries
            );
            hashes.AddRange(split);
        }

        var distinctHashes = hashes
            .Where(hash => !string.IsNullOrWhiteSpace(hash))
            .Select(hash => hash.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var normalizedHashes = distinctHashes.Select(hash => hash.ToLowerInvariant()).ToList();

        if (normalizedHashes.Count == 0)
        {
            await Send.StringAsync("Ok.", cancellation: ct);
            return;
        }

        var movieTasksTask = _dbContext
            .DownloadTaskMovieFile.Where(x => x.HashId != null && normalizedHashes.Contains(x.HashId.ToLower()))
            .Select(x => new DownloadTaskLookup(x.Id, x.DownloadStatus))
            .ToListAsync(ct);

        var episodeTasksTask = _dbContext
            .DownloadTaskTvShowEpisodeFile.Where(x => x.HashId != null && normalizedHashes.Contains(x.HashId.ToLower()))
            .Select(x => new DownloadTaskLookup(x.Id, x.DownloadStatus))
            .ToListAsync(ct);

        await Task.WhenAll(movieTasksTask, episodeTasksTask);

        var downloadTasks = movieTasksTask.Result.Concat(episodeTasksTask.Result).ToList();
        if (downloadTasks.Count == 0)
        {
            await Send.StringAsync("Ok.", cancellation: ct);
            return;
        }

        var allTaskIds = new HashSet<Guid>();
        var downloadingTaskIds = new HashSet<Guid>();

        foreach (var downloadTask in downloadTasks)
        {
            allTaskIds.Add(downloadTask.Id);

            if (IsDownloadingStatus(downloadTask.Status))
                downloadingTaskIds.Add(downloadTask.Id);
        }

        foreach (var downloadTaskId in downloadingTaskIds)
        {
            var stopResult = await _commandExecutor.Send(new StopDownloadTaskCommand(downloadTaskId, deleteFiles), ct);
            if (stopResult.IsFailed)
            {
                _log.Here()
                    .Warning(
                        "Failed to stop download task {DownloadTaskId}: {Errors}",
                        downloadTaskId,
                        stopResult.Errors
                    );
            }
        }

        if (allTaskIds.Count > 0)
            await DeleteDownloadTasks(allTaskIds.ToList(), ct);

        await Send.StringAsync("Ok.", cancellation: ct);
    }

    private async Task ClearCompleted(List<Guid> downloadTaskIds, CancellationToken ct)
    {
        var clearResult = await _commandExecutor.Send(new ClearCompletedDownloadTasksCommand(downloadTaskIds), ct);
        if (clearResult.IsFailed)
        {
            _log.Here().Warning("Failed to clear completed download tasks: {Errors}", clearResult.Errors);
        }
    }

    private async Task DeleteDownloadTasks(List<Guid> downloadTaskIds, CancellationToken ct)
    {
        await _dbContext.DownloadTaskMovie.Where(x => downloadTaskIds.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await _dbContext.DownloadTaskMovieFile.Where(x => downloadTaskIds.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await _dbContext.DownloadTaskTvShow.Where(x => downloadTaskIds.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await _dbContext.DownloadTaskTvShowSeason.Where(x => downloadTaskIds.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await _dbContext.DownloadTaskTvShowEpisode.Where(x => downloadTaskIds.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await _dbContext
            .DownloadTaskTvShowEpisodeFile.Where(x => downloadTaskIds.Contains(x.Id))
            .ExecuteDeleteAsync(ct);
    }

    private static bool IsDownloadingStatus(DownloadStatus status) =>
        status is DownloadStatus.Downloading or DownloadStatus.Queued;
}
