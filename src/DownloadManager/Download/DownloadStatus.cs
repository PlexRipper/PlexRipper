
namespace PlexRipper.DownloadManager.Download
{
    public enum DownloadStatus
    {
        // Client object is created and set to default values
        Initialized,

        // Client is waiting for the file to begin downloading
        Waiting,

        // Client is downloading data from the server
        Downloading,

        // Client releases used resources, so the download can be paused
        Pausing,

        // Download is paused
        Paused,

        // Download is added to the queue
        Queued,

        // Client releases used resources, so the download can be deleted
        Deleting,

        // Download is deleted
        Deleted,

        // Download is completed
        Completed,

        // There was an error during download
        Error
    }
}
