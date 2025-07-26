namespace PlexRipper.Domain;

public abstract class BasePlexMediaQuality : BaseEntity
{
    [Column(Order = 5)]
    public required VideoQuality Quality { get; init; }

    [NotMapped]
    public abstract PlexMediaType Type { get; }
}
