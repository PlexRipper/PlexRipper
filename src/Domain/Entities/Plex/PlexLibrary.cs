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
    /// Gets the total file size of the nested media.
    /// </summary>
    [Column(Order = 10)]
    public long MediaSize { get; private set; } = 0;

    /// <summary>
    /// Gets the total <see cref="PlexMovie"/> count.
    /// </summary>
    [Column(Order = 11)]
    public int MovieCount { get; private set; } = 0;

    /// <summary>
    /// Gets the total <see cref="PlexTvShow"/> count.
    /// </summary>
    [Column(Order = 12)]
    public int TvShowCount { get; private set; } = 0;

    /// <summary>
    /// Gets the total <see cref="PlexTvShowSeason"/> count of all <see cref="PlexTvShow">PlexTvShows</see> in this library.
    /// </summary>
    [Column(Order = 13)]
    public int SeasonCount { get; private set; } = 0;

    /// <summary>
    /// Gets the total <see cref="PlexTvShowEpisode"/> count of all <see cref="PlexTvShow">PlexTvShows</see> in this library.
    /// </summary>
    [Column(Order = 14)]
    public int EpisodeCount { get; private set; } = 0;

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

    [NotMapped]
    public string Name => Title;

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
    /// Gets a value indicating whether this <see cref="PlexLibrary"/> has been updated since it was last synced with PlexRipper.
    /// </summary>
    [NotMapped]
    public bool Outdated => SyncedAt < UpdatedAt;

    public void SetMovieMetaData(int movieCount, long mediaSize)
    {
        MovieCount = movieCount;
        MediaSize = mediaSize;

        TvShowCount = 0;
        SeasonCount = 0;
        EpisodeCount = 0;
    }

    public void SetTvShowMetaData(int tvShowCount, int seasonCount, int episodeCount, long mediaSize)
    {
        TvShowCount = tvShowCount;
        SeasonCount = seasonCount;
        EpisodeCount = episodeCount;
        MediaSize = mediaSize;

        MovieCount = 0;
    }

    #endregion
}
