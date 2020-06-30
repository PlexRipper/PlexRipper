using System.ComponentModel.DataAnnotations;

namespace PlexRipper.Domain.Entities.Base
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }
    }
}
