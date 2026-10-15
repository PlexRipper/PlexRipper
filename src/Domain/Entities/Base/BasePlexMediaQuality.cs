namespace Reaparr.Domain;

public abstract class BasePlexMediaQuality : BaseEntity
{
    public required VideoQuality VideoQuality { get; init; }

    [NotMapped]
    public abstract PlexMediaType Type { get; }
}
