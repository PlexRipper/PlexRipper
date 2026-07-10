namespace Reaparr.Data.Contracts;

public interface IMediaQueryCache
{
    Task<Result<PagedMediaQueryResult>> GetMediaAsync(MediaQueryFilter filter, CancellationToken cancellationToken);

    Task BuildCache();
    
    void InvalidateLibrary(int plexLibraryId, string reason);

    void InvalidateLibraries(IReadOnlyCollection<int> plexLibraryIds, string reason);
}
