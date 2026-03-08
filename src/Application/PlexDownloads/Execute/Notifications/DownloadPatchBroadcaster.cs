using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Reaparr.Data.Contracts;
using Reaparr.SignalR.Contracts;

namespace Reaparr.Application;

public class DownloadPatchBroadcaster : BackgroundService, IDownloadPatchBroadcaster
{
    private readonly ILogger _log;
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly IDownloadHubService _downloadHubService;
    private readonly Channel<ImmediatePatchRequest> _immediateChannel;
    private readonly ConcurrentDictionary<ProgressScopeKey, ConcurrentDictionary<Guid, byte>> _dirtyProgressByScope;
    private readonly ConcurrentDictionary<int, long> _sequenceByServer;
    private readonly ConcurrentDictionary<Guid, DownloadStatus> _latestStatusByNode;

    public DownloadPatchBroadcaster(
        ILogger log,
        IReaparrDbContextFactory dbContextFactory,
        IDownloadHubService downloadHubService
    )
    {
        _log = log.ForContext<DownloadPatchBroadcaster>();
        _dbContextFactory = dbContextFactory;
        _downloadHubService = downloadHubService;

        _immediateChannel = Channel.CreateUnbounded<ImmediatePatchRequest>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false }
        );
        _dirtyProgressByScope = new ConcurrentDictionary<ProgressScopeKey, ConcurrentDictionary<Guid, byte>>();
        _sequenceByServer = new ConcurrentDictionary<int, long>();
        _latestStatusByNode = new ConcurrentDictionary<Guid, DownloadStatus>();
    }

    public bool TryMarkStatusChanged(Guid nodeId, DownloadStatus status)
    {
        if (_latestStatusByNode.TryGetValue(nodeId, out var previousStatus) && previousStatus == status)
            return false;

        _latestStatusByNode[nodeId] = status;
        return true;
    }

    public Task MarkProgressDirtyAsync(
        int plexServerId,
        DownloadTaskKey rootKey,
        Guid changedNodeId,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var scope = ProgressScopeKey.From(plexServerId, rootKey);
        var changedByNode = _dirtyProgressByScope.GetOrAdd(scope, _ => new ConcurrentDictionary<Guid, byte>());
        changedByNode.TryAdd(changedNodeId, 0);
        return Task.CompletedTask;
    }

    public async Task PublishImmediateStatusPatchAsync(
        int plexServerId,
        DownloadTaskKey rootKey,
        IReadOnlyCollection<Guid> changedNodeIds,
        CancellationToken cancellationToken = default
    )
    {
        if (changedNodeIds.Count == 0)
            return;

        var request = new ImmediatePatchRequest(ProgressScopeKey.From(plexServerId, rootKey), changedNodeIds);
        await _immediateChannel.Writer.WriteAsync(request, cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var immediateTask = ProcessImmediateQueueAsync(stoppingToken);
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
            _immediateChannel.Writer.TryComplete();
            await immediateTask;
        }
    }

    private async Task ProcessImmediateQueueAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var request in _immediateChannel.Reader.ReadAllAsync(stoppingToken))
            {
                await SendPatchAsync(request.Scope, request.ChangedNodeIds, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // expected on shutdown
        }
    }

    private async Task FlushProgressAsync(CancellationToken cancellationToken)
    {
        var scopeSnapshot = _dirtyProgressByScope.Keys.ToList();
        foreach (var scope in scopeSnapshot)
        {
            if (!_dirtyProgressByScope.TryRemove(scope, out var changedNodes) || changedNodes.Count == 0)
                continue;

            await SendPatchAsync(scope, changedNodes.Keys.ToList(), cancellationToken);
        }
    }

    private async Task SendPatchAsync(
        ProgressScopeKey scope,
        IReadOnlyCollection<Guid> changedNodeIds,
        CancellationToken cancellationToken
    )
    {
        if (changedNodeIds.Count == 0)
            return;

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
    }

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

    private sealed record ImmediatePatchRequest(ProgressScopeKey Scope, IReadOnlyCollection<Guid> ChangedNodeIds);

    private sealed record ProgressScopeKey(int PlexServerId, DownloadTaskKey RootKey)
    {
        public static ProgressScopeKey From(int plexServerId, DownloadTaskKey rootKey) =>
            new(
                plexServerId,
                new DownloadTaskKey
                {
                    Id = rootKey.Id,
                    PlexServerId = rootKey.PlexServerId,
                    PlexLibraryId = rootKey.PlexLibraryId,
                    Type = rootKey.Type,
                }
            );
    }
}
