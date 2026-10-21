namespace Reaparr.Application.Contracts;

/// <summary>
/// Carries the selectors and effective destination used to build a download preview.
/// </summary>
public record DownloadPreviewRequest
{
    public required List<DownloadMediaDTO> DownloadMedias { get; init; } = [];

    public int? DestinationFolderPathId { get; init; }

    public string CustomDestinationFolderPath { get; init; } = string.Empty;
}
