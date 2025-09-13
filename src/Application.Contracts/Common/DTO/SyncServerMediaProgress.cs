namespace Reaparr.Application.Contracts;

public class SyncServerMediaProgress
{
    public required int ServerId { get; init; }

    public required List<LibraryProgress> LibraryProgresses { get; init; }

    public decimal Percentage => LibraryProgresses.Average(x => x.Percentage);
}
