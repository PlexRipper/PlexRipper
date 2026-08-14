namespace Reaparr.Application;

internal record ComparisonDetailsRow
{
    public required int RowId { get; init; }

    public int? ParentRowId { get; init; }

    public required int Level { get; init; }

    public required int PlexMediaId { get; init; }

    public required PlexMediaType Type { get; init; }

    public required string Title { get; init; }

    public required int ComparisonId { get; init; }

    public required bool IsActionable { get; init; }

    public VideoQuality RemoteQuality { get; init; }

    public VideoQuality OwnedQuality { get; init; }

    public string RemoteLocation { get; init; } = string.Empty;

    public string OwnedLocation { get; init; } = string.Empty;

    public int RemotePlexLibraryId { get; init; }

    public int RemotePlexServerId { get; init; }
}
