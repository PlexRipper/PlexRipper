using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class PlexLibrary : BaseEntity
{
    #region Properties

    /// <summary>
    /// Gets or sets plex Library type, see: https://github.com/Arcanemagus/plex-api/wiki/MediaTypes.
    /// </summary>
    [Column(Order = 1)]
    public required PlexMediaType Type { get; init; }

    /// <summary>
    /// Gets or sets the display title of this <see cref="PlexLibrary"/>.
    /// </summary>
    [Column(Order = 2)]
    public required string Title { get; init; }

    /// <summary>
    /// Gets or sets the Library Section Identifier used by Plex.
    /// </summary>
    [Column(Order = 3)]
    public required string Key { get; init; }

    /// <summary>
    /// Gets or sets the creation date of this <see cref="PlexLibrary"/> on the <see cref="PlexServer"/> by the owner.
    /// Value is set by the PlexApi.
    /// </summary>
    [Column(Order = 5)]
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets or sets the last time this <see cref="PlexLibrary"/> was updated by the <see cref="PlexServer"/> owner.
    /// Value is set by the PlexApi.
    /// </summary>
    [Column(Order = 6)]
    public required DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last time this <see cref="PlexLibrary"/> was scanned for new media by the <see cref="PlexServer"/> owner.
    /// Value is set by the PlexApi.
    /// </summary>
    [Column(Order = 7)]
    public required DateTime ScannedAt { get; init; }

    /// <summary>
    /// Gets or sets the DateTime this <see cref="PlexLibrary"/> was last synced with the PlexApi.
    /// </summary>
    [Column(Order = 8)]
    public DateTime? SyncedAt { get; set; }

    /// <summary>
    /// Gets or sets the unique id of the <see cref="PlexLibrary"/>.
    /// </summary>
    [Column(Order = 9)]
    public required Guid Uuid { get; init; }

    /// <summary>
    /// Gets or sets the <see cref="PlexLibraryMetaData"/>, this is a JSON field that contains a collection
    /// of various values that don't warrant their own database column.
    /// </summary>
    [Column(Order = 11)]
    public required PlexLibraryMetaData MetaData { get; set; }

    #endregion

    #region Relationships

    /// <summary>
    /// Gets or sets the PlexServer this PlexLibrary belongs to.
    /// </summary>
    public PlexServer? PlexServer { get; init; }

    /// <summary>
    /// Gets or sets the PlexServerId of the PlexServer this PlexLibrary belongs to.
    /// </summary>
    public required int PlexServerId { get; set; }

    /// <summary>
    /// Gets or sets the default download destination <see cref="FolderPath"/>.
    /// </summary>
    public FolderPath? DefaultDestination { get; init; }

    /// <summary>
    /// Gets or sets the Id of the Default Destination <see cref="FolderPath"/>.
    /// </summary>
    public int? DefaultDestinationId { get; set; }

    public List<PlexMovie> Movies { get; set; } = [];

    public List<PlexTvShow> TvShows { get; set; } = [];

    public List<PlexAccountLibrary> PlexAccountLibraries { get; init; } = [];

    #endregion

    #region Helpers

    /// <summary>
    /// Gets whether this <see cref="PlexLibrary"/> has any media assigned.
    /// </summary>
    [NotMapped]
    public bool HasMedia => MediaCount > 0;

    [NotMapped]
    public int MediaCount
    {
        get
        {
            return Type switch
            {
                PlexMediaType.Movie => MovieCount,
                PlexMediaType.TvShow => TvShowCount,
                _ => -1,
            };
        }
    }

    /// <summary>
    /// Gets the total filesize of the nested media.
    /// Will return -1 when MetaData is invalid.
    /// </summary>
    [NotMapped]
    public long MediaSize => MetaData?.MediaSize ?? -1;

    /// <summary>
    /// Gets the current <see cref="PlexMovie"/> count.
    /// Will return -1 if the <see cref="PlexMediaType"/> of this library does not match the count requested,
    /// or if MetaData is invalid.
    /// E.g. There will be a -1 when this library is of type TvShow.
    /// </summary>
    [NotMapped]
    public int MovieCount => Type == PlexMediaType.Movie ? MetaData?.MovieCount ?? -1 : -1;

    /// <summary>
    /// Gets the current <see cref="PlexTvShow"/> count.
    /// Will return -1 if the <see cref="PlexMediaType"/> of this library does not match the count requested,
    /// or if MetaData is invalid.
    /// E.g. There will be a -1 when this library is of type Movie.
    /// </summary>
    [NotMapped]
    public int TvShowCount => Type == PlexMediaType.TvShow ? MetaData?.TvShowCount ?? -1 : -1;

    /// <summary>
    /// Gets the current <see cref="PlexTvShowSeason"/> count.
    /// Will return -1 if the <see cref="PlexMediaType"/> of this library does not match the count requested.
    /// or if MetaData is invalid.
    /// E.g. There will be a -1 when this library is of type Movie.
    /// </summary>
    [NotMapped]
    public int SeasonCount => Type == PlexMediaType.TvShow ? MetaData?.TvShowSeasonCount ?? -1 : -1;

    /// <summary>
    /// Gets the current <see cref="PlexTvShowEpisode"/> count.
    /// Will return -1 if the <see cref="PlexMediaType"/> of this library does not match the count requested.
    /// or if MetaData is invalid.
    /// E.g. There will be a -1 when this library is of type Movie.
    /// </summary>
    [NotMapped]
    public int EpisodeCount => Type == PlexMediaType.TvShow ? MetaData?.TvShowEpisodeCount ?? -1 : -1;

    [NotMapped]
    public string Name => Title;

    /// <summary>
    /// Gets a value indicating whether this <see cref="PlexLibrary"/> has been updated since it was last synced with PlexRipper.
    /// </summary>
    [NotMapped]
    public bool Outdated => SyncedAt < UpdatedAt;

    #endregion
}
