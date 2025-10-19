using MessagePack;
using MessagePack.Formatters;

namespace Reaparr.Application;

/// <summary>
/// NOTE Changing the order of the Key(n) has to be corrected as well in the mapper in signalrStore.toServerDownloadProgressDTO() in the front-end
/// </summary>
[MessagePackObject]
public record ServerDownloadProgressMessagePackDTO
{
    [MessagePack.Key(0)]
    public required int Id { get; init; }

    [MessagePack.Key(1)]
    public required int DownloadableTasksCount { get; init; }

    [MessagePack.Key(2)]
    public required List<DownloadProgressMessagePackDTO> Downloads { get; init; } = [];
}

[MessagePackObject]
public record DownloadProgressMessagePackDTO
{
    [MessagePack.Key(0)]
    public required Guid Id { get; init; }

    [MessagePack.Key(1)]
    public required string Title { get; init; }

    [MessagePack.Key(2)]
    [MessagePackFormatter(typeof(EnumAsStringFormatter<PlexMediaType>))]
    public required PlexMediaType MediaType { get; init; }

    [MessagePack.Key(3)]
    [MessagePackFormatter(typeof(EnumAsStringFormatter<DownloadStatus>))]
    public required DownloadStatus Status { get; init; }

    [MessagePack.Key(4)]
    public required decimal Percentage { get; init; }

    [MessagePack.Key(5)]
    public required long DataReceived { get; init; }

    [MessagePack.Key(6)]
    public required long DataTotal { get; init; }

    [MessagePack.Key(7)]
    public required long DownloadSpeed { get; init; }

    [MessagePack.Key(8)]
    public required long TimeRemaining { get; init; }

    [MessagePack.Key(9)]
    public required List<DownloadProgressMessagePackDTO> Children { get; init; } = [];
}
