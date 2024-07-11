namespace Application.Contracts;

public class PlexMediaQualityDTO
{
    public string Quality { get; set; }

    public string DisplayQuality { get; set; }

    // TODO Pre-calculate this to avoid doing it every time, used to identify the different media files
    public string HashId { get; set; }
}
