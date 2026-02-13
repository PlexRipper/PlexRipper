using FluentResults;
using Reaparr.Domain;

namespace Reaparr.BackgroundJobs.Contracts;

public record LibraryProgress
{
    public required int PlexLibraryId { get; init; }

    public required PlexMediaType PlexLibraryType { get; init; }

    public required IReadOnlyList<LibraryProgressItem> Items { get; init; }

    public IReadOnlyList<IError> Errors { get; set; }

    public TimeSpan TimeRemaining => Items.Aggregate(TimeSpan.Zero, (acc, i) => acc + i.TimeRemaining);

    public int Received => Items.Sum(i => i.Received);

    public int Total => Items.Sum(i => i.Total);

    public decimal Percentage => DataFormat.GetPercentage(Received, Total);

    public DateTime TimeStamp { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets a value indicating whether the <see cref="PlexLibrary"/> has finished refreshing.
    /// All items must be complete for the overall progress to be considered complete.
    /// </summary>
    public bool IsComplete => Items.All(i => i.IsComplete);
}
