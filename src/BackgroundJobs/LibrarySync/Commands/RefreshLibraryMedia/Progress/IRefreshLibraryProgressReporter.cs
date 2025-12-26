namespace Reaparr.BackgroundJobs;

public interface IRefreshLibraryProgressReporter
{
    Task SendProgress(RefreshLibraryProgressUpdate update);
}
