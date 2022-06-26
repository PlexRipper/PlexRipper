using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain
{
    /// <summary>
    /// Used for the merging and transferring of the completed downloaded media file.
    /// </summary>
    public class DownloadFileTask : BaseEntity
    {
        #region Properties

        public DateTime CreatedAt { get; set; }

        public string FilePathsCompressed { get; set; }

        #region Relationships

        public DownloadTask DownloadTask { get; set; }

        public int DownloadTaskId { get; set; }

        #endregion

        #region Helpers

        [NotMapped]
        public string FileName => DownloadTask?.FileName;

        [NotMapped]
        public long FileSize => DownloadTask?.DataTotal ?? -1L;

        /// <summary>
        /// Gets a list of file paths that need to be merged and/or moved.
        /// </summary>
        [NotMapped]
        public List<string> FilePaths => FilePathsCompressed?.Split(';').ToList() ?? new List<string>();

        #endregion

        #endregion
    }
}