namespace Reaparr.Application;

public interface IRefreshLibraryProgressReporter
{
    Task SendProgress(RefreshLibraryProgressUpdate update);
}
