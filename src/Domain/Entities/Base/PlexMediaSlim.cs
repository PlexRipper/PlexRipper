using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class PlexMediaSlim : BaseEntity
{
    [Column(Order = 2)]
    public string Title { get; set; }

    [Column(Order = 3)]
    public int Year { get; set; }

    /// <summary>
    /// This can be empty, in that case it gets the value of <see cref="Title"/>.
    /// </summary>
    [Column(Order = 4)]
    public string SortTitle { get; set; }

    /// <summary>
    /// Gets or sets the duration in seconds of the (nested) media.
    /// </summary>
    [Column(Order = 5)]
    public int Duration { get; set; }

    /// <summary>
    /// Gets or sets the total filesize of the nested media.
    /// </summary>
    [Column(Order = 6)]
    public long MediaSize { get; set; }

    /// <summary>
    /// Gets or sets the number of direct children
    /// E.G. if the type is tvShow, then this number would be the season count, if season then this would be the episode count.
    /// </summary>
    [Column(Order = 12)]
    public int ChildCount { get; set; }

    /// <summary>
    /// Gets or sets when this media was added to the Plex library.
    /// </summary>
    [Column(Order = 13)]
    public DateTime AddedAt { get; set; }

    /// <summary>
    /// Gets or sets when this media was last updated in the Plex library.
    /// </summary>
    [Column(Order = 14)]
    public DateTime UpdatedAt { get; set; }

    public int PlexLibraryId { get; set; }

    public int PlexServerId { get; set; }

    [NotMapped]
    public virtual PlexMediaType Type { get; set; }
}