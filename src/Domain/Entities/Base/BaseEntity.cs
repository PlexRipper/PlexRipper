namespace Reaparr.Domain;

public abstract class BaseEntity
{
    [Key]
    [Column(Order = 0)]
    public int Id { get; set; }

    /// <summary>
    /// Set all navigation properties to null to avoid unneeded database operation with these properties.
    /// </summary>
    public virtual void SetNull() { }
}
