namespace Reaparr.SignalR.Contracts;

public record LibrarySyncProgressDTO
{
    public required int PlexLibraryId { get; init; }

    public required PlexMediaType PlexLibraryType { get; init; }

    public required TimeSpan TimeRemaining { get; set; }

    public required IReadOnlyList<LibrarySyncProgressItemDTO> Items { get; init; }

    public int Received => Items.Sum(i => i.Received);

    public int Total => Items.Sum(i => i.Total);

    public decimal Percentage => DataFormat.GetPercentage(Received, Total);

    public DateTime TimeStamp { get; } = DateTime.UtcNow;

    public required IReadOnlyList<IError> Errors { get; set; }

    /// <summary>
    /// Gets a value indicating whether the <see cref="PlexLibrary"/> has finished refreshing.
    /// All items must be complete for the overall progress to be considered complete.
    /// </summary>
    public bool IsComplete => Items.All(i => i.IsComplete);
}

public record LibrarySyncProgressItemDTO
{
    public required PlexMediaType MediaType { get; init; }

    public required int Received { get; init; }

    public required int Total { get; init; }

    public required TimeSpan TimeRemaining { get; init; }

    public decimal Percentage => DataFormat.GetPercentage(Received, Total);

    public bool IsComplete => Received >= Total;
}
