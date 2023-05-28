namespace PlexRipper.FileSystem;

public class FileMergeFinishedNotification : INotification
{
    public int FileTaskId { get; }

    public FileMergeFinishedNotification(int fileTaskId)
    {
        FileTaskId = fileTaskId;
    }
}