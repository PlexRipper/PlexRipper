using PlexRipper.Domain;

namespace WebAPI.Contracts;

public class LibraryProgress
{
    public LibraryProgress() { }

    public LibraryProgress(int plexLibraryId, int received, int total, bool isRefreshing = true)
    {
        Id = plexLibraryId;
        Received = received;
        Total = total;
        Percentage = DataFormat.GetPercentage(received, total);
        IsRefreshing = isRefreshing;
        IsComplete = received >= total;
        TimeStamp = DateTime.UtcNow;
    }

    public int Id { get; set; }

    public decimal Percentage { get; set; }

    public int Received { get; set; }

    public int Total { get; set; }

    public DateTime TimeStamp { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="PlexLibrary"/> is currently refreshing the data from the external PlexServer
    /// or from our own local database.
    /// </summary>

    public bool IsRefreshing { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="PlexLibrary"/> has finished refreshing.
    /// </summary>

    public bool IsComplete { get; set; }
}
