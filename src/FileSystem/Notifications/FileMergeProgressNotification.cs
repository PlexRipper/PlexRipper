namespace PlexRipper.FileSystem;

public class FileMergeProgressNotification : INotification
{
    public FileMergeProgress Progress { get; }

    public FileMergeProgressNotification(FileMergeProgress progress)
    {
        Progress = progress;
    }
}