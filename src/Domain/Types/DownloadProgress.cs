namespace PlexRipper.Domain.Types
{
    public class DownloadProgress
    {
        public int Id { get; set; }

        public string Status { get; set; }

        public decimal Percentage { get; set; }

        public string DownloadSpeed { get; set; }

        public long DataReceived { get; set; }
        public long DataTotal { get; set; }

    }
}
