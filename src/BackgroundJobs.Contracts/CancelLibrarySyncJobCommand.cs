namespace Reaparr.BackgroundJobs.Contracts;

public record CancelLibrarySyncJobCommand(int PlexLibraryId) : ICommand<Result>;
