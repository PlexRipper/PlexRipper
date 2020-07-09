namespace PlexRipper.Domain.Types
{
    public class DownloadProgress
    {
        public int Id { get; set; }

        public string Status { get; set; }

        public decimal Percentage { get; set; }

        public string DataReceived { get; set; }

        public string DataTotal { get; set; }

        public string DownloadSpeed { get; set; }
    }
}
