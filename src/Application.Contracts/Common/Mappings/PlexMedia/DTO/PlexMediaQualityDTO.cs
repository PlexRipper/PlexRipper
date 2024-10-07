namespace Application.Contracts;

public class PlexMediaQualityDTO
{
    public string Quality { get; init; }

    public string DisplayQuality { get; init; }

    // TODO Pre-calculate this to avoid doing it every time, used to identify the different media files
    public string HashId { get; init; }
}
