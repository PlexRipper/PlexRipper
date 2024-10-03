using PlexRipper.Domain;

namespace WebAPI.Contracts;

public record LibraryProgress
{
    public required TimeSpan TimeRemaining { get; set; }

    public required int Id { get; init; }

    public required int Step { get; init; }

    public required int TotalSteps { get; init; }

    public required int Received { get; init; }

    public required int Total { get; init; }

    public decimal Percentage => DataFormat.GetPercentage(Received, Total);

    public DateTime TimeStamp { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="PlexLibrary"/> is currently refreshing the data from the external PlexServer
    /// or from our own local database.
    /// </summary>
    public bool IsRefreshing => Step != TotalSteps;

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="PlexLibrary"/> has finished refreshing.
    /// </summary>
    public bool IsComplete => Received >= Total;
}
