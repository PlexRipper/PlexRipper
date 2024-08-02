namespace PlexRipper.Domain;

public record PlexMediaQuality
{
    public required string Quality { get; init; }

    public string DisplayQuality
    {
        get
        {
            return Quality switch
            {
                "sd" => "480p",
                "480" => "480p",
                "576" => "576p",
                "720" => "720p",
                "1080" => "1080p",
                "1440" => "1440p",
                "2160" => "4k",
                "4k" => "4k",
                _ => "?",
            };
        }
    }

    // TODO Pre-calculate this to avoid doing it every time, used to identify the different media files
    public required string HashId { get; init; }
}
