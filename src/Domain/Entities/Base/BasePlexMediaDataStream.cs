namespace Reaparr.Domain;

public abstract class BasePlexMediaDataStream : BaseEntity
{
    /// <summary>
    /// Gets or sets the typed <see cref="StreamType"/> classification reported by Plex for this
    /// source stream.
    /// </summary>
    [Column(Order = 1)]
    public required StreamType StreamType { get; init; }

    /// <summary>
    /// Gets or sets the internal database identifier of the owning media-data row. This owner
    /// foreign key is distinct from both the Plex source media/part identity and this stream's
    /// internal <see cref="BaseEntity.Id"/>.
    /// </summary>
    [Column(Order = 2)]
    public required int MediaDataId { get; init; }

    /// <summary>
    /// Gets or sets Plex's server-scoped source identifier for this stream, used to reconstruct
    /// <c>/library/streams/{PlexApiStreamId}</c>. It is distinct from the owner foreign key
    /// <see cref="MediaDataId"/> and the internal <see cref="BaseEntity.Id"/>.
    /// </summary>
    [Column(Order = 3)]
    public required int PlexApiStreamId { get; init; }

    /// <summary>
    /// Gets or sets the internal foreign key to the normalized <see cref="StreamLanguage"/> lookup
    /// row. The normalized language code and tag are exposed through <see cref="Language"/>.
    /// </summary>
    [Column(Order = 4)]
    public required int LanguageId { get; init; }

    /// <summary>
    /// Gets or sets whether the stream is external rather than embedded. Null means unknown;
    /// the current SDK supplies no authoritative classification. This is not a file-availability flag.
    /// </summary>
    [Column(Order = 5)]
    public bool? IsExternal { get; init; }

    /// <summary>
    /// Gets or sets the normalized companion subtitle or lyrics content format. This is a format
    /// label, not a filesystem path, URL, or indication that a sidecar is available.
    /// </summary>
    [Column(Order = 6)]
    public string? SidecarFormat { get; init; }

    /// <summary>
    /// Gets or sets the normalized language lookup navigation associated with <see cref="LanguageId"/>.
    /// It describes the stream's language code and tag, not owner or source identity.
    /// </summary>
    [Column(Order = 7)]
    public StreamLanguage Language { get; init; } = null!;
}
