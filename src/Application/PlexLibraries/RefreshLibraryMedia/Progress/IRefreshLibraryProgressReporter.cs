namespace PlexRipper.Application;

public interface IRefreshLibraryProgressReporter
{
    void SendProgress(RefreshLibraryProgressUpdate update);
}