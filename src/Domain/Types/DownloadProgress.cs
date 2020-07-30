namespace PlexRipper.Domain.Types
{
    public class DownloadProgress
    {
        public int Id { get; set; }

        public string Status { get; set; }

        public decimal Percentage { get; set; }

        public long DownloadSpeed { get; set; }

        public long DataReceived { get; set; }
        public long DataTotal { get; set; }

        /// <summary>
        /// The download time remaining in seconds
        /// </summary>
        public int TimeRemaining { get; set; }

    }
}
