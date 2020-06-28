using PlexRipper.Domain.Entities.Base;

namespace PlexRipper.Domain.Entities
{
    public class FolderPath : BaseEntity
    {
        public string DisplayName { get; set; }
        public string Directory { get; set; }
    }
}
