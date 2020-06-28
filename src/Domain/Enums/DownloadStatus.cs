
namespace PlexRipper.Domain.Enums
{
    public enum DownloadStatus
    {
        /// <summary>
        /// Client object is created and set to default values
        /// </summary>
        Initialized,

        /// <summary>
        /// Client is waiting for the file to begin downloading
        /// </summary>
        Waiting,

        /// <summary>
        /// Client is downloading data from the server
        /// </summary>
        Downloading,

        /// <summary>
        /// Client releases used resources, so the download can be paused
        /// </summary>
        Pausing,

        /// <summary>
        /// Download is paused
        /// </summary>
        Paused,

        /// <summary>
        /// Download is added to the queue
        /// </summary>
        Queued,

        /// <summary>
        /// Client releases used resources, so the download can be deleted
        /// </summary>
        Deleting,

        /// <summary>
        /// Download is deleted
        /// </summary>
        Deleted,

        /// <summary>
        /// Download is completed
        /// </summary>
        Completed,

        /// <summary>
        /// There was an error during download
        /// </summary>
        Error,

        /// <summary>
        /// String value was unable to be parsed to this enum.
        /// </summary>
        Unknown
    }
}
