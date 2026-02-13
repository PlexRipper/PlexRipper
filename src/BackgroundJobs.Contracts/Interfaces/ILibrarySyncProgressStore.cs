using FluentResults;
using Reaparr.Domain;

namespace Reaparr.BackgroundJobs.Contracts;

public interface ILibrarySyncProgressStore
{
    LibraryProgress? Get(int plexLibraryId);

    Task StartAsync(int plexLibraryId, PlexMediaType type);

    Task UpdateItemAsync(int plexLibraryId, LibraryProgressItem item);

    Task UpdateErrorAsync(int plexLibraryId, Result errorResult);

    void Remove(int plexLibraryId);
}
