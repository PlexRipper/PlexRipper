using PlexRipper.Domain;

namespace WebAPI.Contracts;

public record LibraryProgress
{
    public required TimeSpan TimeRemaining { get; set; }

    public required int Id { get; set; }
    public required int Step { get; set; }

    public decimal Percentage => DataFormat.GetPercentage(Received, Total);

    public required int Received { get; set; }

    public required int Total { get; set; }

    public DateTime TimeStamp { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="PlexLibrary"/> is currently refreshing the data from the external PlexServer
    /// or from our own local database.
    /// </summary>
    public required bool IsRefreshing { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="PlexLibrary"/> has finished refreshing.
    /// </summary>
    public bool IsComplete => Received >= Total;
}
