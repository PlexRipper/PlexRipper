using PlexRipper.Domain;

namespace PlexApi.Contracts;

public record MediaSyncProgress
{
    public required PlexMediaType Type { get; init; }

    public required int Total { get; init; }

    public required int Received { get; init; }

    /// <summary>
    /// Gets the percentage of the received items compared to the total items.
    /// E.g: 0.5 = 50%
    /// </summary>
    public decimal Percentage => DataFormat.GetPercentage(Received, Total) / 100;

    public required TimeSpan TimeRemaining { get; init; } = TimeSpan.Zero;
}
