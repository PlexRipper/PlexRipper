namespace PlexRipper.Domain.Entities
{
    public class PlexLibrary : BaseEntity
    {
        public int SectionId { get; set; }

        public int Count { get; set; }

        public string Key { get; set; }

        public string Title { get; set; }

        public bool HasAccess { get; set; }

        public virtual PlexServer PlexServer { get; set; }

    }
}
