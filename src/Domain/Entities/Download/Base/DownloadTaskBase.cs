using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public abstract class DownloadTaskBase : BaseEntityGuid
{
    /// <summary>
    /// Gets or sets the unique identifier used by the Plex Api to keep track of media.
    /// This is only unique on that specific server.
    /// </summary>
    [Column(Order = 1)]
    public required int Key { get; init; }

    /// <summary>
    /// Gets or sets the media display title.
    /// </summary>
    [Column(Order = 2)]
    public required string Title { get; set; }

    /// <summary>
    ///  Gets or sets the current download state of this DownloadTask.
    /// </summary>
    [Column(Order = 8)]
    public required DownloadStatus DownloadStatus { get; set; }

    [Column(Order = 10)]
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets or sets the full formatted media title, based on the <see cref="PlexMediaType"/>.
    /// E.g. "TvShow/Season/Episode".
    /// </summary>
    [Column(Order = 14)]
    public required string FullTitle { get; set; }

    #region Relationships

    public PlexServer? PlexServer { get; init; }

    public required int PlexServerId { get; set; }

    public PlexLibrary? PlexLibrary { get; init; }

    public required int PlexLibraryId { get; set; }

    #endregion

    #region Helpers

    public abstract PlexMediaType MediaType { get; }

    public abstract DownloadTaskType DownloadTaskType { get; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="DownloadTaskGeneric"/> is downloadable.
    /// e.g. A episode or movie part, an episode or movie without parts.
    /// </summary>
    public abstract bool IsDownloadable { get; }

    public abstract int Count { get; }

    /// <summary>
    /// Returns the parent <see cref="DownloadTaskKey"/> of this <see cref="DownloadTaskBase"/>.
    /// </summary>
    /// <returns></returns>
    public abstract DownloadTaskKey? ToParentKey();

    #endregion

    public DownloadTaskKey ToKey() =>
        new()
        {
            Type = DownloadTaskType,
            Id = Id,
            PlexServerId = PlexServerId,
            PlexLibraryId = PlexLibraryId,
        };
}
