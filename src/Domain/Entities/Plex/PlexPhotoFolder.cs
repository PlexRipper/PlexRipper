namespace Reaparr.Domain;

/// <summary>
/// A source-root directory or descendant folder in a Plex photo library.
/// Roots have no parent and retain source identity and availability independently of album membership.
/// </summary>
public class PlexPhotoFolder : BaseEntity
{
    /// <summary>
    /// ID of the Plex server that owns this folder.
    /// </summary>
    public required int PlexServerId { get; set; }

    /// <summary>
    /// ID of the photo library that contains this folder.
    /// </summary>
    public required int PlexLibraryId { get; set; }

    /// <summary>
    /// ID of the containing folder, or null for a source root.
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// Navigation to the containing folder; absent for a source root.
    /// </summary>
    public PlexPhotoFolder? Parent { get; set; }

    /// <summary>
    /// Immediate descendant folders, excluding assets.
    /// </summary>
    public ICollection<PlexPhotoFolder> Children { get; set; } = [];

    /// <summary>
    /// Image and clip assets assigned directly to this folder.
    /// </summary>
    public ICollection<PlexPhotoMediaData> Files { get; set; } = [];

    /// <summary>
    /// Folder path relative to its source root; empty for the source-root row itself.
    /// </summary>
    public required string RelativePath { get; set; }

    /// <summary>
    /// Display name of the folder or source root.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Stable identity used to reconcile a source root across refreshes; null for descendants.
    /// </summary>
    public string? RootIdentityKey { get; set; }

    /// <summary>
    /// Source directory path on the Plex server for a root; null for descendants.
    /// </summary>
    public string? SourceRootPath { get; set; }

    /// <summary>
    /// Plex location identifier for a source root, when supplied; null for descendants.
    /// </summary>
    public long? PlexApiLocationId { get; set; }

    /// <summary>
    /// Whether a source root is currently active. Retained roots can be inactive; descendants store null.
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Navigation to the photo library that contains this folder.
    /// </summary>
    public PlexLibrary? PlexLibrary { get; set; }

    /// <summary>
    /// Navigation to the Plex server that owns this folder.
    /// </summary>
    public PlexServer? PlexServer { get; set; }
}
