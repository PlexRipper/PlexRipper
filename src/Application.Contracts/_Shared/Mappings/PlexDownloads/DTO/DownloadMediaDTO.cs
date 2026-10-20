namespace Reaparr.Application.Contracts;

public record DownloadMediaDTO
{
    public required List<int> MediaIds { get; init; } = [];

    public required List<PlexMediaQualityDTO> Qualities { get; init; } = [];

    public required PlexMediaType Type { get; init; }

    public required int PlexServerId { get; init; }

    public required int PlexLibraryId { get; init; }

    /// <summary>
    /// For photo selectors, identifies whether <see cref="MediaIds"/> contains album, folder, or asset IDs.
    /// This is null for every non-photo media family.
    /// </summary>
    public required PhotoItemKind? SelectionKind { get; init; }

    public bool KeepCompletedInDownloadFolder { get; init; }
}
