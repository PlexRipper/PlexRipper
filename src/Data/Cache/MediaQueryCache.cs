using System.Collections.Concurrent;
using System.Diagnostics;
using FlexQuery.NET.Models;
using Reaparr.Application.Contracts;

namespace Reaparr.Data;

public sealed class MediaQueryCache : IMediaQueryCache
{
    private readonly ConcurrentDictionary<MediaQuerySnapshotKey, MediaQuerySnapshot> _snapshots = new();

    private readonly ConcurrentDictionary<MediaQuerySnapshotKey, Lazy<Task<Result<MediaQuerySnapshot>>>>
        _builds = new();

    private readonly ICommandExecutor _commandExecutor;
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly ILogger _log;

    public MediaQueryCache(ILogger log, ICommandExecutor commandExecutor, IReaparrDbContextFactory dbContextFactory)
    {
        _log = log.ForContext<MediaQueryCache>();
        _commandExecutor = commandExecutor;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Result<PagedMediaQueryResult>> GetMediaAsync(MediaQueryFilter filter, CancellationToken cancellationToken) => throw new NotImplementedException();

    public async Task BuildCache() => throw new NotImplementedException();

    public void InvalidateLibrary(int plexLibraryId, string reason)
    {
        throw new NotImplementedException();
    }
    public void InvalidateLibraries(IReadOnlyCollection<int> plexLibraryIds, string reason)
    {
        throw new NotImplementedException();
    }
}