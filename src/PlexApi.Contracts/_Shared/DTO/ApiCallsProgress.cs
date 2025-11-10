namespace Reaparr.PlexApi.Contracts;

public record ApiCallProgress
{
    public required int Total { get; init; }

    public required int Received { get; init; }

    /// <summary>
    /// Gets the percentage of the received items compared to the total items.
    /// E.g: 0.5 = 50%
    /// </summary>
    public decimal Percentage => DataFormat.GetPercentage(Received, Total) / 100;
}
