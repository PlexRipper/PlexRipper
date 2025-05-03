using Application.Contracts;
using WebAPI.Contracts;

namespace PlexRipper.Application;

public record RefreshLibraryProgressUpdate
{
    public required int PlexLibraryId { get; init; }

    public required PlexMediaType PlexLibraryType { get; init; }

    public required int Step { get; init; }

    public required decimal Percentage { get; init; }

    public TimeSpan TimeRemaining { get; init; } = TimeSpan.Zero;

    public required Action<LibraryProgress> Action { get; init; }
}

public class RefreshLibraryProgressReporter : IRefreshLibraryProgressReporter
{
    private readonly ISignalRService _signalRService;

    private const int _baseCountProgress = 1000;
    private const int _totalProgressSteps = 1;

    public RefreshLibraryProgressReporter(ISignalRService signalRService)
    {
        _signalRService = signalRService;
    }

    public async Task SendProgress(RefreshLibraryProgressUpdate update)
    {
        var totalProgressSteps = update.PlexLibraryType switch
        {
            PlexMediaType.TvShow => 5,
            PlexMediaType.Movie => 3,
            _ => _totalProgressSteps,
        };

        var countStep = (decimal)_baseCountProgress / totalProgressSteps;
        var index = countStep * update.Step + countStep * update.Percentage;

        var progress = new LibraryProgress
        {
            TimeRemaining = update.TimeRemaining,
            Id = update.PlexLibraryId,
            Step = update.Step,
            Received = (int)Math.Floor(index),
            Total = _baseCountProgress,
            TotalSteps = totalProgressSteps,
        };

        update.Action.Invoke(progress);

        await _signalRService.SendLibraryProgressUpdateAsync(progress);
    }
}
