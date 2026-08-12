namespace Reaparr.Application.Contracts;

public interface ILibrarySyncProgressStore
{
    LibraryProgress? Get(int plexLibraryId);

    Task StartAsync(int plexLibraryId, PlexMediaType type, CancellationToken cancellationToken = default);

    Task UpdateItemAsync(int plexLibraryId, LibraryProgressItem item, CancellationToken cancellationToken = default);

    Task UpdateErrorAsync(int plexLibraryId, Result errorResult, CancellationToken cancellationToken = default);
}
