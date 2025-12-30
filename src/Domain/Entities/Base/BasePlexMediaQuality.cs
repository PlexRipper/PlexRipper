namespace Reaparr.Domain;

public abstract class BasePlexMediaQuality : BaseEntity
{
    [Column(Order = 1)]
    public required VideoQuality Quality { get; init; }

    [NotMapped]
    public abstract PlexMediaType Type { get; }
}
