namespace Reaparr.Data.Contracts;

public interface IMediaQueryCache
{
    Task<Result<PagedMediaQueryResult>> GetMediaAsync(MediaQueryFilter filter, CancellationToken cancellationToken);

    Task BuildCache(CancellationToken cancellationToken = default);
    
    void InvalidateLibrary(int plexLibraryId, string reason);

    void InvalidateLibraries(IReadOnlyCollection<int> plexLibraryIds, string reason);

    /// <summary>
    /// When true, InvalidateLibrary / InvalidateLibraries are no-ops.
    /// Used during startup to suppress cache-doom loops while library sync storms are in progress.
    /// </summary>
    bool SuppressInvalidation { get; set; }
}
