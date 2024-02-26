using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public abstract class DownloadTaskBase
{
    [Key]
    [Column(Order = 0)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier used by the Plex Api to keep track of media.
    /// This is only unique on that specific server.
    /// </summary>
    [Column(Order = 1)]
    public int Key { get; set; }

    /// <summary>
    /// The total size of the file in bytes.
    /// </summary>
    [Column(Order = 6)]
    public long DataTotal { get; set; }

    [Column(Order = 8)]
    public DownloadStatus DownloadStatus { get; set; }

    [Column(Order = 10)]
    public DateTime Created { get; set; }

    #region Relationships

    public PlexServer PlexServer { get; set; }

    public int PlexServerId { get; set; }

    public PlexLibrary PlexLibrary { get; set; }

    public int PlexLibraryId { get; set; }

    #endregion

    #region Helpers

    /// <summary>
    /// Set all navigation properties to null to avoid unneeded database operation with these properties.
    /// </summary>
    public virtual void SetNull()
    {
        PlexServer = null;
        PlexLibrary = null;
    }

    public abstract PlexMediaType MediaType { get; }

    public abstract DownloadTaskType DownloadTaskType { get; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="DownloadTask"/> is downloadable.
    /// e.g. A episode or movie part, an episode or movie without parts.
    /// </summary>
    public abstract bool IsDownloadable { get; }

    #endregion

    #region Compare

    public bool Equals(PlexTvShow tvShow) => tvShow is not null && PlexServerId == tvShow.PlexServerId && MediaType == tvShow.Type && Key == tvShow.Key;

    public bool Equals(PlexTvShowSeason season) => season is not null && PlexServerId == season.PlexServerId && MediaType == season.Type && Key == season.Key;

    public bool Equals(PlexTvShowEpisode episode) =>
        episode is not null && PlexServerId == episode.PlexServerId && MediaType == episode.Type && Key == episode.Key;

    #endregion
}