using Reaparr.Domain;

namespace Reaparr.BackgroundJobs.Contracts;

public record LibraryProgress
{
    public required int PlexLibraryId { get; init; }

    public required PlexMediaType PlexLibraryType { get; init; }

    public required TimeSpan TimeRemaining { get; set; }

    public required IReadOnlyList<LibraryProgressItem> Items { get; init; }

    public int Received => Items.Sum(i => i.Received);

    public int Total => Items.Sum(i => i.Total);

    public decimal Percentage => DataFormat.GetPercentage(Received, Total);

    public DateTime TimeStamp { get; } = DateTime.UtcNow;

    public string ErrorText { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the <see cref="PlexLibrary"/> has finished refreshing.
    /// All items must be complete for the overall progress to be considered complete.
    /// </summary>
    public bool IsComplete => Items.All(i => i.IsComplete);
}
