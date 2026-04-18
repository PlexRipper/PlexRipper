using System.Diagnostics.CodeAnalysis;

namespace Reaparr.SignalR.Contracts;

public record AppUpdateDownloadProgressDTO
{
    [SetsRequiredMembers]
    public AppUpdateDownloadProgressDTO(int percentage)
    {
        Percentage = percentage;
    }

    public required int Percentage { get; init; }

    public bool IsComplete => Percentage >= 100;
}
