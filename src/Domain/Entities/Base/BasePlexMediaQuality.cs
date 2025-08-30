namespace Reaparr.Domain;

public abstract class BasePlexMediaQuality : BaseEntity
{
    [Column(Order = 3)]
    public required VideoQuality Quality { get; init; }

    [NotMapped]
    public abstract PlexMediaType Type { get; }
}
