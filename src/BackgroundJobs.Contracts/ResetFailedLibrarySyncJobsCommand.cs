namespace Reaparr.BackgroundJobs.Contracts;

public record ResetFailedLibrarySyncJobsCommand(int PlexServerId) : ICommand<Result>;
