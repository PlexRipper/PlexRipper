namespace PlexRipper.Domain.Types.FileSystem
{
    public class FileMergeProgress
    {
        public int FileTaskId { get; set; }

        public long BytesTransferred { get; set; }

        public long BytesTotal { get; set; }
    }
}