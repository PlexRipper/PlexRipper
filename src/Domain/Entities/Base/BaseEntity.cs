using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain.Entities.Base
{
    public abstract class BaseEntity
    {
        [Key]
        [Column(Order = 0)]
        public int Id { get; set; }
    }
}
