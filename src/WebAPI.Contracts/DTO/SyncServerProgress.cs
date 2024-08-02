using System.Diagnostics.CodeAnalysis;
using PlexRipper.Domain;

namespace WebAPI.Contracts;

public class SyncServerProgress
{
    [SetsRequiredMembers]
    public SyncServerProgress(int serverId, List<LibraryProgress> libraryProgresses)
    {
        Id = serverId;
        LibraryProgresses = libraryProgresses;
        Percentage = DataFormat.GetPercentage(
            LibraryProgresses.Sum(x => x.Received),
            LibraryProgresses.Sum(x => x.Total)
        );
    }

    public required int Id { get; set; }

    public required decimal Percentage { get; set; }

    public required List<LibraryProgress> LibraryProgresses { get; set; }
}
