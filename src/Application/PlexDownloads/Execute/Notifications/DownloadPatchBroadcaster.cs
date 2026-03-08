using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Reaparr.Data.Contracts;
using Reaparr.SignalR.Contracts;

namespace Reaparr.Application;

/// <summary>
/// Coordinates download task update ingestion and dispatches optimized patch updates to the front-end.
/// It handles immediate status changes and buffered progress updates with a 1-second flush cadence.
/// </summary>
public class DownloadTaskUpdateDispatcher : BackgroundService, IDownloadTaskUpdateDispatcher
{
    private readonly ILogger _log;
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly IDownloadHubService _downloadHubService;
    private readonly Channel<ImmediatePatchRequest> _statusChannel;
    private readonly ConcurrentDictionary<ProgressScopeKey, BufferedProgressUpdate> _progressByScope;
    private readonly ConcurrentDictionary<int, long> _sequenceByServer;
    private readonly ConcurrentDictionary<Guid, DownloadStatus> _latestStatusByNode;

    /// <summary>
    /// Creates a new dispatcher instance.
    /// </summary>
    public DownloadTaskUpdateDispatcher(
        ILogger log,
        IReaparrDbContextFactory dbContextFactory,
        IDownloadHubService downloadHubService
    )
    {
        _log = log.ForContext<DownloadTaskUpdateDispatcher>();
        _dbContextFactory = dbContextFactory;
        _downloadHubService = downloadHubService;

        _statusChannel = Channel.CreateUnbounded<ImmediatePatchRequest>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false }
        );
        _progressByScope = new ConcurrentDictionary<ProgressScopeKey, BufferedProgressUpdate>();
        _sequenceByServer = new ConcurrentDictionary<int, long>();
        _latestStatusByNode = new ConcurrentDictionary<Guid, DownloadStatus>();
    }

    /// <inheritdoc />
    public async Task<Result> OnStatusChangedAsync(
        DownloadTaskKey key,
        DownloadStatus newStatus,
        CancellationToken cancellationToken = default
    )
    {
        var result = await Result.Try(async Task () =>
        {
            using var dbContext = await _dbContextFactory.CreateAsync();
            var currentStatus = await dbContext.GetDownloadStatusAsync(key, cancellationToken);
            var hasStatusChanged = currentStatus != newStatus;

            if (hasStatusChanged)
            {
                await dbContext.SetDownloadStatus(key, newStatus);
                _latestStatusByNode[key.Id] = newStatus;
            }

            var changedParentKeys = await dbContext.DetermineDownloadStatus(key, cancellationToken);
            var rootKey = await dbContext.GetRootDownloadTaskKeyAsync(key, cancellationToken);
            if (rootKey is null)
                return;

            var changedNodeIds = changedParentKeys.Select(x => x.Id).Append(key.Id).Distinct().ToList();
            if (hasStatusChanged || changedParentKeys.Count > 0)
            {
                await _statusChannel.Writer.WriteAsync(
                    new ImmediatePatchRequest(ProgressScopeKey.From(rootKey), changedNodeIds),
                    cancellationToken
                );
            }

            var scope = ProgressScopeKey.From(rootKey);
            _progressByScope.AddOrUpdate(
                scope,
                _ => BufferedProgressUpdate.FromStatus(key),
                (_, current) => current with { NodeId = key.Id, Key = key }
            );
        });

        if (result.IsFailed)
            result.LogError();

        return result.LogIfFailed();
    }

    /// <inheritdoc />
    public Result OnProgressUpdated(
        DownloadTaskKey key,
        DownloadTaskProgress progress,
        DirectDownloadSnapshot? snapshot = null
    )
    {
        return Result.Try(() =>
        {
            var scope = ProgressScopeKey.From(key);
            var update = new BufferedProgressUpdate
            {
                NodeId = key.Id,
                Key = key,
                Progress = progress,
                Snapshot = snapshot,
            };

            _progressByScope.AddOrUpdate(scope, _ => update, (_, __) => update);
        });
    }

    /// <summary>
    /// Runs the background processing loop for immediate status patches and periodic progress flushes.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var immediateTask = ProcessStatusQueueAsync(stoppingToken);
        var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        try
        {
            while (await periodicTimer.WaitForNextTickAsync(stoppingToken))
            {
                await FlushProgressAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // expected on shutdown
        }
        finally
        {
            periodicTimer.Dispose();
            _statusChannel.Writer.TryComplete();
            await immediateTask;
        }
    }

    /// <summary>
    /// Reads immediate status patch requests from the channel and dispatches them to clients.
    /// </summary>
    private async Task ProcessStatusQueueAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var request in _statusChannel.Reader.ReadAllAsync(stoppingToken))
            {
                var sendResult = await SendPatchAsync(request.Scope, request.ChangedNodeIds, stoppingToken);
                if (sendResult.IsFailed)
                    sendResult.LogError();
            }
        }
        catch (OperationCanceledException)
        {
            // expected on shutdown
        }
    }

    /// <summary>
    /// Flushes buffered progress updates, persists the latest values, and emits leaf-only progress patches.
    /// </summary>
    private async Task FlushProgressAsync(CancellationToken cancellationToken)
    {
        var scopeSnapshot = _progressByScope.Keys.ToList();
        if (scopeSnapshot.Count == 0)
            return;

        using var dbContext = await _dbContextFactory.CreateAsync();
        foreach (var scope in scopeSnapshot)
        {
            if (!_progressByScope.TryRemove(scope, out var bufferedProgress))
                continue;

            var flushResult = await Result.Try(async Task () =>
            {
                if (bufferedProgress.Progress is not null)
                {
                    await dbContext.UpdateDownloadProgress(
                        bufferedProgress.Key,
                        bufferedProgress.Progress,
                        bufferedProgress.Snapshot,
                        cancellationToken
                    );
                }

                var patchMeta = await dbContext.GetDownloadPatchMetaAsync(bufferedProgress.Key, cancellationToken);
                if (patchMeta is null)
                    return;

                _latestStatusByNode[bufferedProgress.NodeId] = patchMeta.Value.Status;

                var progressPatch = bufferedProgress.ToPatch(patchMeta.Value.ParentId, patchMeta.Value.Status);
                if (progressPatch is not null)
                {
                    var sequence = _sequenceByServer.AddOrUpdate(scope.PlexServerId, 1, (_, current) => current + 1);
                    await _downloadHubService.SendDownloadPatchAsync(
                        scope.PlexServerId,
                        sequence,
                        [progressPatch],
                        deletedIds: null,
                        cancellationToken
                    );
                }
            });

            if (flushResult.IsFailed)
            {
                _log.Here()
                    .Error(
                        "Failed to flush buffered progress for {DownloadTaskKey}. Requeuing latest value.",
                        bufferedProgress.Key
                    );
                flushResult.LogError();
                _progressByScope.AddOrUpdate(scope, _ => bufferedProgress, (_, __) => bufferedProgress);
            }
        }
    }

    /// <summary>
    /// Builds and sends a status patch containing changed nodes and impacted ancestors.
    /// </summary>
    private async Task<Result> SendPatchAsync(
        ProgressScopeKey scope,
        IReadOnlyCollection<Guid> changedNodeIds,
        CancellationToken cancellationToken
    )
    {
        if (changedNodeIds.Count == 0)
            return Result.Ok();

        var result = await Result.Try(async Task () =>
        {
            using var dbContext = await _dbContextFactory.CreateAsync();
            var rootTasks = await dbContext.GetDownloadProgressRootTasksAsync([scope.RootKey], cancellationToken);
            var rootTask = rootTasks.FirstOrDefault();
            if (rootTask is null)
                return;

            var allNodes = Flatten([rootTask]).ToDictionary(x => x.Id);
            var patchNodeIds = ResolvePatchNodeIds(changedNodeIds, allNodes);

            var upserts = patchNodeIds
                .Select(id => allNodes[id])
                .Select(node => new DownloadPatchDTO
                {
                    Id = node.Id,
                    ParentId = node.ParentId,
                    Status = node.DownloadStatus,
                    Percentage = node.Percentage,
                    DataReceived = node.DataReceived,
                    DataTotal = node.DataTotal,
                    DownloadSpeed = node.Speed,
                    TimeRemaining = node.TimeRemaining,
                })
                .ToList();

            if (upserts.Count == 0)
                return;

            var sequence = _sequenceByServer.AddOrUpdate(scope.PlexServerId, 1, (_, current) => current + 1);
            await _downloadHubService.SendDownloadPatchAsync(
                scope.PlexServerId,
                sequence,
                upserts,
                deletedIds: null,
                cancellationToken
            );
        });

        return result;
    }

    /// <summary>
    /// Resolves changed nodes and all ancestor nodes that must be updated in the client tree.
    /// </summary>
    private static IReadOnlyCollection<Guid> ResolvePatchNodeIds(
        IReadOnlyCollection<Guid> changedNodeIds,
        IReadOnlyDictionary<Guid, DownloadTaskGeneric> allNodes
    )
    {
        var patchNodeIds = new HashSet<Guid>();

        foreach (var changedNodeId in changedNodeIds)
        {
            if (!allNodes.TryGetValue(changedNodeId, out var node))
                continue;

            patchNodeIds.Add(node.Id);

            var parentId = node.ParentId;
            while (parentId != Guid.Empty && allNodes.TryGetValue(parentId, out var parentNode))
            {
                patchNodeIds.Add(parentNode.Id);
                parentId = parentNode.ParentId;
            }
        }

        return patchNodeIds;
    }

    /// <summary>
    /// Flattens a hierarchical download task collection into a depth-first sequence.
    /// </summary>
    private static IEnumerable<DownloadTaskGeneric> Flatten(IEnumerable<DownloadTaskGeneric> tasks)
    {
        foreach (var task in tasks)
        {
            yield return task;

            if (task.Children.Count == 0)
                continue;

            foreach (var child in Flatten(task.Children))
                yield return child;
        }
    }
}
