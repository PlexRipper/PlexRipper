namespace PlexRipper.Domain.Entities.JoinTables
{
    /// <summary>
    /// This is a join table entity that will return the specific libraries the PlexAccount has access to.
    /// Cases may happen where multiple PlexAccounts might have access to the same <see cref="PlexServer"/> but not the same <see cref="PlexLibrary"/>.
    /// </summary>
    public class PlexAccountLibrary
    {
        public int PlexAccountId { get; set; }
        public virtual PlexAccount PlexAccount { get; set; }
        public int PlexLibraryId { get; set; }
        public virtual PlexLibrary PlexLibrary { get; set; }
    }
}
