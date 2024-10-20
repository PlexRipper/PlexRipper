namespace PlexRipper.Domain;

public record DownloadTaskDirectory
{
    public required string DownloadRootPath { get; set; }

    public required string DestinationRootPath { get; set; }

    public required string MovieFolder { get; set; }

    public required string TvShowFolder { get; set; }

    public required string SeasonFolder { get; set; }
}
