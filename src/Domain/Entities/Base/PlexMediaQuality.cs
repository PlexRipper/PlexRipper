namespace PlexRipper.Domain;

public class PlexMediaQuality
{
    public string Quality { get; set; }

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
    public string HashId { get; set; }
}
