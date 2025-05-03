namespace PlexRipper.Domain;

public class BasePlexMediaDataPart
{
    /// <summary>
    /// Indicates if the part is accessible.
    /// </summary>
    public required bool? Accessible { get; set; }

    /// <summary>
    /// Indicates if the part exists.
    /// </summary>
    public required bool? Exists { get; set; }

    /// <summary>
    /// Unique part identifier.
    /// </summary>
    public required long Id { get; set; }

    /// <summary>
    /// Key to access this part.
    /// </summary>
    public required string Key { get; set; }

    public required string? Indexes { get; set; }

    /// <summary>
    /// Duration of the part in milliseconds.
    /// </summary>
    public required int Duration { get; set; }

    /// <summary>
    /// File path for the part.
    /// </summary>
    public required string File { get; set; }

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public required long Size { get; set; }

    /// <summary>
    /// Container format of the part.
    /// </summary>
    public required string Container { get; set; }

    /// <summary>
    /// Video profile for the part.
    /// </summary>
    public required string VideoProfile { get; set; }

    public required string AudioProfile { get; set; }
}
