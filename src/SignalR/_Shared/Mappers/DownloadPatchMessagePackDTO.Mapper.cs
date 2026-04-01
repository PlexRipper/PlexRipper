namespace Reaparr.SignalR;

public static class DownloadPatchMessagePackMapper
{
    public static DownloadPatchMessagePackDTO ToMessagePack(
        int serverId,
        long sequence,
        IReadOnlyCollection<DownloadPatchDTO> upserts,
        IReadOnlyCollection<Guid>? deletedIds = null
    ) =>
        new()
        {
            ServerId = serverId,
            Sequence = sequence,
            Upserts = upserts.Select(ToMessagePack).ToList(),
            DeletedIds = deletedIds?.ToList() ?? [],
        };

    private static DownloadPatchEntryMessagePackDTO ToMessagePack(DownloadPatchDTO source) =>
        new()
        {
            Id = source.Id,
            ParentId = source.ParentId,
            Status = source.Status,
            Percentage = source.Percentage,
            DataReceived = source.DataReceived,
            DataTotal = source.DataTotal,
            DownloadSpeed = source.DownloadSpeed,
            TimeRemaining = source.TimeRemaining,
        };
}
