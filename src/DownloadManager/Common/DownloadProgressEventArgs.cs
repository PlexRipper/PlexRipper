namespace PlexRipper.DownloadManager.Common
{
    public class DownloadProgressEventArgs
    {
        public DownloadProgressEventArgs(long bytesReceived, long totalBytesToReceive)
        {
            TotalBytesToReceive = totalBytesToReceive;
            BytesReceived = bytesReceived;
        }

        /// <summary>Gets the number of bytes received.</summary>
        /// <returns>An <see cref="T:System.Int64" /> value that indicates the number of bytes received.</returns>
        public long BytesReceived { get; }

        /// <summary>Gets the total number of bytes in a <see cref="T:System.Net.WebClient" /> data download operation.</summary>
        /// <returns>An <see cref="T:System.Int64" /> value that indicates the number of bytes that will be received.</returns>
        public long TotalBytesToReceive { get; }

        // Percentage of downloaded data
        public float Percentage => BytesReceived / (float)TotalBytesToReceive * 100F;
    }
}