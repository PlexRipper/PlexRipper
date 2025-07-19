namespace PlexRipper.Domain;

public abstract class BaseEntityGuid
{
    [Key]
    [Column(Order = 0)]
    public required Guid Id { get; init; }
}
