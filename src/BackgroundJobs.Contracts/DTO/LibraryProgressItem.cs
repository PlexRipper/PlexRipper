using Reaparr.Domain;

namespace Reaparr.BackgroundJobs.Contracts;

public record LibraryProgressItem
{
    public required PlexMediaType MediaType { get; init; }

    public required int Received { get; init; }

    public required int Total { get; init; }

    public required TimeSpan TimeRemaining { get; init; }

    public decimal Percentage => DataFormat.GetPercentage(Received, Total);

    public bool IsComplete => Received >= Total;
}
