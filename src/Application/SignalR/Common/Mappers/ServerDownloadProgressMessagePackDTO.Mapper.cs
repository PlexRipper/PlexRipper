using Reaparr.Application.Contracts;

namespace Reaparr.Application;

public static class ServerDownloadProgressMapper
{
    public static ServerDownloadProgressMessagePackDTO ToMessagePack(this ServerDownloadProgressDTO source) =>
        new()
        {
            Id = source.Id,
            DownloadableTasksCount = source.DownloadableTasksCount,
            Downloads = source.Downloads.Select(d => d.ToMessagePack()).ToList(),
        };

    public static DownloadProgressMessagePackDTO ToMessagePack(this DownloadProgressDTO source) =>
        new()
        {
            Id = source.Id,
            Title = source.Title,
            MediaType = source.MediaType,
            Status = source.Status,
            Percentage = source.Percentage,
            DataReceived = source.DataReceived,
            DataTotal = source.DataTotal,
            DownloadSpeed = source.DownloadSpeed,
            TimeRemaining = source.TimeRemaining,
            Children = source.Children.Select(c => c.ToMessagePack()).ToList(),
        };
}
