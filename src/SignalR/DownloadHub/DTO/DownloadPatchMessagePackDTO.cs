using MessagePack;
using MessagePack.Formatters;

namespace Reaparr.SignalR;

[MessagePackObject]
public record DownloadPatchMessagePackDTO
{
    [MessagePack.Key(0)]
    public required int ServerId { get; init; }

    [MessagePack.Key(1)]
    public required long Sequence { get; init; }

    [MessagePack.Key(2)]
    public required List<DownloadPatchEntryMessagePackDTO> Upserts { get; init; }

    [MessagePack.Key(3)]
    public required List<Guid> DeletedIds { get; init; }
}

[MessagePackObject]
public record DownloadPatchEntryMessagePackDTO
{
    [MessagePack.Key(0)]
    public required Guid Id { get; init; }

    [MessagePack.Key(1)]
    public required Guid ParentId { get; init; }

    [MessagePack.Key(2)]
    [MessagePackFormatter(typeof(EnumAsStringFormatter<DownloadStatus>))]
    public required DownloadStatus Status { get; init; }

    [MessagePack.Key(3)]
    public required decimal Percentage { get; init; }

    [MessagePack.Key(4)]
    public required long DataReceived { get; init; }

    [MessagePack.Key(5)]
    public required long DataTotal { get; init; }

    [MessagePack.Key(6)]
    public required long DownloadSpeed { get; init; }

    [MessagePack.Key(7)]
    public required int TimeRemaining { get; init; }
}
