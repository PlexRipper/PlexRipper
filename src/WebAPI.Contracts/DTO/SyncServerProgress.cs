using PlexRipper.Domain;

namespace WebAPI.Contracts;

public class SyncServerProgress
{
    public SyncServerProgress() { }

    public SyncServerProgress(int serverId, List<LibraryProgress> libraryProgresses)
    {
        Id = serverId;
        LibraryProgresses = libraryProgresses;
        Percentage = DataFormat.GetPercentage(LibraryProgresses.Sum(x => x.Received), LibraryProgresses.Sum(x => x.Total));
    }

    public int Id { get; set; }

    public decimal Percentage { get; set; }

    public List<LibraryProgress> LibraryProgresses { get; set; }
}