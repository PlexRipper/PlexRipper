namespace PlexRipper.Application;

public interface IRefreshLibraryProgressReporter
{
    Task SendProgress(RefreshLibraryProgressUpdate update);
}