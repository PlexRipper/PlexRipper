namespace Reaparr.Domain;

public abstract class BaseEntity
{
    [Key]
    [Column(Order = 0)]
    public int Id { get; set; }
}
