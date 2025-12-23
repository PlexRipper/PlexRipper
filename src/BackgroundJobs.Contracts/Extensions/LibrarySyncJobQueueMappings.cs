using Reaparr.Domain;

namespace Reaparr.BackgroundJobs.Contracts;

public static class LibrarySyncJobQueueMappings
{
    public static LibrarySyncJobQueueDTO ToDTO(this LibrarySyncJobQueue source) =>
        new()
        {
            Priority = source.Priority,
            Status = source.Status,
            CreatedAt = source.CreatedAt,
            StartedAt = source.StartedAt,
            CompletedAt = source.CompletedAt,
            ErrorMessage = source.ErrorMessage,
            PlexLibraryId = source.PlexLibraryId,
            PlexServerId = source.PlexServerId,
        };
}


