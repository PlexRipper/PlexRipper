using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public abstract class BaseEntityGuid
{
    [Key]
    [Column(Order = 0)]
    public required Guid Id { get; init; }
}
