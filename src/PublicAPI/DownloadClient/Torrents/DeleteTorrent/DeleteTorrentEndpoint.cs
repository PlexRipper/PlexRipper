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
                || raw.Split(['|', ',', ';', ' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Length > 0
            )
            .WithMessage("Hashes must be 'all' or a delimited list of hashes.");
    }
}

public sealed class DeleteTorrentEndpoint : Endpoint<DeleteTorrentRequest>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

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

        var normalizedHashes = ParseHashes(req);

        if (normalizedHashes is { Count: 0 })
        {
            await Send.StringAsync("Ok.", cancellation: ct);
            return;
        }

        var (downloadingKeys, completedKeys, allKeys) = await QueryKeysByStatus(normalizedHashes, ct);

        if (allKeys.Count == 0)
        {
            await Send.StringAsync("Ok.", cancellation: ct);
            return;
        }

        var deleteFiles = req.DeleteFiles ?? true;

        foreach (var key in downloadingKeys)
        {
            var stopResult = await _commandExecutor.Send(new StopDownloadTaskCommand(key.Id, deleteFiles), ct);
            if (stopResult.IsFailed)
                _log.Here().Warning("Failed to stop download task {DownloadTaskKey}: {Errors}", key, stopResult.Errors);
        }

        // StopDownloadTaskCommand skips file deletion for completed tasks (they are in phase Completed).
        // Explicitly delete their source files from the download directory here.
        if (deleteFiles && completedKeys.Count > 0)
        {
            var deleteFilesResult = await _commandExecutor.Send(new DeleteDownloadTaskFilesCommand(completedKeys), ct);
            if (deleteFilesResult.IsFailed)
                _log.Here()
                    .Warning("Failed to delete download files for completed tasks: {Errors}", deleteFilesResult.Errors);
        }

        var deleteResult = await _commandExecutor.Send(new DeleteDownloadTasksByKeyCommand(allKeys), ct);
        if (deleteResult.IsFailed)
            _log.Here().Warning("Failed to delete download tasks: {Errors}", deleteResult.Errors);

        await Send.StringAsync("Ok.", cancellation: ct);
    }

    /// <summary>
    /// Parses the request into a normalized lower-case hash list.
    /// Returns <c>null</c> when "all" is requested (caller passes <c>null</c> to query all tasks).
    /// Returns an empty list when no parseable hashes are present (caller should bail early).
    /// </summary>
    private static List<string>? ParseHashes(DeleteTorrentRequest req)
    {
        if (string.Equals(req.HashesRaw, "all", StringComparison.OrdinalIgnoreCase))
            return null;

        var hashes = new List<string>();
        if (req.Hashes?.Count > 0)
            hashes.AddRange(req.Hashes);

        if (!string.IsNullOrWhiteSpace(req.HashesRaw))
            hashes.AddRange(
                req.HashesRaw.Split(['|', ',', ';', ' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            );

        return hashes
            .Where(h => !string.IsNullOrWhiteSpace(h))
            .Select(h => h.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Queries the two file-task tables and returns all matched keys split by status.
    /// Pass <c>null</c> for <paramref name="normalizedHashes"/> to match all tasks with a HashId.
    /// Returns all matched keys regardless of status — callers stop active ones and delete all.
    /// </summary>
    private async Task<(List<DownloadTaskKey> Downloading, List<DownloadTaskKey> Completed, List<DownloadTaskKey> All)>
        QueryKeysByStatus(List<string>? normalizedHashes, CancellationToken ct)
    {
        static bool IsDownloading(DownloadStatus s) => s is DownloadStatus.Downloading or DownloadStatus.Queued;

        var movieTask = _dbContext
            .DownloadTaskMovieFile.Where(x =>
                x.HashId != null && (normalizedHashes == null || normalizedHashes.Contains(x.HashId.ToLower()))
            )
            .Select(x => new
            {
                Key = new DownloadTaskKey
                {
                    Id = x.Id,
                    Type = x.DownloadTaskType,
                    PlexServerId = x.PlexServerId,
                    PlexLibraryId = x.PlexLibraryId,
                },
                x.DownloadStatus,
            })
            .ToListAsync(ct);

        var episodeTask = _dbContext
            .DownloadTaskTvShowEpisodeFile.Where(x =>
                x.HashId != null && (normalizedHashes == null || normalizedHashes.Contains(x.HashId.ToLower()))
            )
            .Select(x => new
            {
                Key = new DownloadTaskKey
                {
                    Id = x.Id,
                    Type = x.DownloadTaskType,
                    PlexServerId = x.PlexServerId,
                    PlexLibraryId = x.PlexLibraryId,
                },
                x.DownloadStatus,
            })
            .ToListAsync(ct);

        await Task.WhenAll(movieTask, episodeTask);

        var all = movieTask.Result.Concat(episodeTask.Result).ToList();
        return (
            all.Where(x => IsDownloading(x.DownloadStatus)).Select(x => x.Key).ToList(),
            all.Where(x => x.DownloadStatus == DownloadStatus.Completed).Select(x => x.Key).ToList(),
            all.Select(x => x.Key).ToList()
        );
    }
}
