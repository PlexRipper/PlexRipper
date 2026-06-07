namespace Reaparr.Domain;

public abstract class BasePlexMediaQuality : BaseEntity
{
    public required VideoQuality Quality { get; init; }

    [NotMapped]
    public abstract PlexMediaType Type { get; }
}
