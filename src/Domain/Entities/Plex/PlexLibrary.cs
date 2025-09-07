namespace Reaparr.Domain;

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
    /// NOTE: Value is set by the PlexApi.
    /// </summary>
    [Column(Order = 5)]
    public required DateTime? CreatedAt { get; init; }

    /// <summary>
    /// Gets or sets the last time this <see cref="PlexLibrary"/> was updated by the <see cref="PlexServer"/> owner.
    /// NOTE: Value is set by the PlexApi.
    /// </summary>
    [Column(Order = 6)]
    public required DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last time this <see cref="PlexLibrary"/> was scanned for new media by the <see cref="PlexServer"/> owner.
    /// NOTE: Value is set by the PlexApi.
    /// </summary>
    [Column(Order = 7)]
    public required DateTime? ScannedAt { get; init; }

    /// <summary>
    /// Gets or sets the DateTime this <see cref="PlexLibrary"/> had its media last synced with the PlexApi.
    /// </summary>
    [Column(Order = 8)]
    public DateTime? SyncedAt { get; set; }

    /// <summary>
    /// Gets or sets the unique id of the <see cref="PlexLibrary"/>.
    /// Can be a valid GUID or a Plex generated UUID.
    /// </summary>
    [Column(Order = 9)]
    public required string Uuid { get; init; }

    [Column(Order = 10)]
    public required string Language { get; init; }

    /// <summary>
    /// Gets the total file size of the nested media.
    /// </summary>
    [Column(Order = 11)]
    public long MediaSize { get; init; }

    /// <summary>
    /// Gets the total <see cref="PlexMovie"/> count.
    /// </summary>
    [Column(Order = 12)]
    public int MovieCount { get; init; }

    /// <summary>
    /// Gets the total <see cref="PlexTvShow"/> count.
    /// </summary>
    [Column(Order = 13)]
    public int TvShowCount { get; init; }

    /// <summary>
    /// Gets the total <see cref="PlexTvShowSeason"/> count of all <see cref="PlexTvShow">PlexTvShows</see> in this library.
    /// </summary>
    [Column(Order = 14)]
    public int SeasonCount { get; init; }

    /// <summary>
    /// Gets the total <see cref="PlexTvShowEpisode"/> count of all <see cref="PlexTvShow">PlexTvShows</see> in this library.
    /// </summary>
    [Column(Order = 15)]
    public int EpisodeCount { get; init; }

    [Column(Order = 16)]
    public int ActorsCount { get; init; }

    [Column(Order = 17)]
    public int GenresCount { get; init; }

    [Column(Order = 18)]
    public int CountriesCount { get; init; }

    public int MediaCount { get; private set;}

    #endregion

    #region Relationships

    /// <summary>
    /// Gets or sets the PlexServer this PlexLibrary belongs to.
    /// </summary>
    public PlexServer? PlexServer { get; set; }

    /// <summary>
    /// Gets or sets the PlexServerId of the PlexServer this PlexLibrary belongs to.
    /// </summary>
    public required int PlexServerId { get; set; }

    /// <summary>
    /// Gets or sets the default download destination <see cref="FolderPath"/>.
    /// </summary>
    public FolderPath? DefaultDestination { get; set; }

    /// <summary>
    /// Gets or sets the id of the Default Destination <see cref="FolderPath"/>.
    /// Is only set if the user has diverted from the default <see cref="FolderPath"/> by the <see cref="PlexMediaType">Library Type</see>
    /// </summary>
    public int? DefaultDestinationId { get; set; }

    public ICollection<PlexMovie> Movies { get; private set; } = [];

    public ICollection<PlexTvShow> TvShows { get; private set; } = [];

    public ICollection<PlexAccountLibrary> PlexAccountLibraries { get; private set; } = [];

    public ICollection<PlexActor> Actors { get; set; } = [];

    public ICollection<PlexGenre> Genres { get; set; } = [];

    public ICollection<PlexCountry> Countries { get; set; } = [];

    #endregion

    #region Helpers

    [NotMapped]
    public string Name => Title;

    /// <summary>
    /// Gets a value indicating whether this <see cref="PlexLibrary"/> has been updated since it was last synced with Reaparr.
    /// </summary>
    [NotMapped]
    public bool Outdated => SyncedAt < UpdatedAt;

    #endregion
}
