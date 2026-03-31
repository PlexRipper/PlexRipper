using System.Collections.Concurrent;
using Reaparr.BackgroundJobs.Contracts;
using Reaparr.SignalR.Contracts;

namespace Reaparr.BackgroundJobs;

public class LibrarySyncProgressStore : ILibrarySyncProgressStore
{
    private readonly ILogger _log;
    private readonly IProgressHubService _progressHubService;
    private readonly ConcurrentDictionary<int, LibraryProgress> _store = new();

    public LibrarySyncProgressStore(ILogger logger, IProgressHubService progressHubService)
    {
        _log = logger.ForContext<LibrarySyncProgressStore>();
        _progressHubService = progressHubService;
    }

    public LibraryProgress? Get(int plexLibraryId) =>
        _store.TryGetValue(plexLibraryId, out var progress) ? progress : null;

    public async Task StartAsync(int plexLibraryId, PlexMediaType type, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<LibraryProgressItem> items;

        if (type == PlexMediaType.Movie)
        {
            items =
            [
                new LibraryProgressItem
                {
                    MediaType = PlexMediaType.Movie,
                    Received = 0,
                    Total = 0,
                    TimeRemaining = TimeSpan.Zero,
                },
            ];
        }
        else if (type == PlexMediaType.TvShow)
        {
            items =
            [
                new LibraryProgressItem
                {
                    MediaType = PlexMediaType.TvShow,
                    Received = 0,
                    Total = 0,
                    TimeRemaining = TimeSpan.Zero,
                },
                new LibraryProgressItem
                {
                    MediaType = PlexMediaType.Season,
                    Received = 0,
                    Total = 0,
                    TimeRemaining = TimeSpan.Zero,
                },
                new LibraryProgressItem
                {
                    MediaType = PlexMediaType.Episode,
                    Received = 0,
                    Total = 0,
                    TimeRemaining = TimeSpan.Zero,
                },
            ];
        }
        else
        {
            _log.Here()
                .Warning("Unsupported PlexMediaType {PlexMediaType} for library {PlexLibraryId}", type, plexLibraryId);
            return;
        }

        var progress = new LibraryProgress
        {
            PlexLibraryId = plexLibraryId,
            PlexLibraryType = type,
            Items = items,
        };

        _store[plexLibraryId] = progress;

        // Send initial progress update to clients
        await SendProgressUpdateAsync(progress, cancellationToken);
    }

    public async Task UpdateItemAsync(
        int plexLibraryId,
        LibraryProgressItem item,
        CancellationToken cancellationToken = default
    )
    {
        _store.AddOrUpdate(
            plexLibraryId,
            _ => throw new InvalidOperationException("Library not initialized."),
            (_, existing) =>
            {
                // Upsert: replace if the media type already exists, otherwise append
                var replaced = false;
                var updatedItems = existing
                    .Items.Select(i =>
                    {
                        if (i.MediaType != item.MediaType)
                            return i;
                        replaced = true;
                        return item;
                    })
                    .ToList();

                if (!replaced)
                    updatedItems.Add(item);

                return existing with
                {
                    Items = updatedItems,
                };
            }
        );

        await SendProgressUpdateAsync(plexLibraryId, cancellationToken);
    }

    public async Task UpdateErrorAsync(
        int plexLibraryId,
        Result errorResult,
        CancellationToken cancellationToken = default
    )
    {
        var updated = _store.AddOrUpdate(
            plexLibraryId,
            _ => throw new InvalidOperationException("Library not initialized."),
            (_, existing) => existing with { Errors = errorResult.Errors }
        );

        await SendProgressUpdateAsync(updated, cancellationToken);

        _store.TryRemove(plexLibraryId, out _);
    }

    private async Task SendProgressUpdateAsync(int plexLibraryId, CancellationToken cancellationToken = default)
    {
        if (!_store.TryGetValue(plexLibraryId, out var progress))
        {
            _log.Here()
                .Warning(
                    "Attempted to send progress update for unknown library with ID {PlexLibraryId}",
                    plexLibraryId
                );

            return;
        }

        await SendProgressUpdateAsync(progress, cancellationToken);
    }

    private async Task SendProgressUpdateAsync(LibraryProgress progress, CancellationToken cancellationToken = default)
    {
        var dto = new LibrarySyncProgressDTO
        {
            PlexLibraryId = progress.PlexLibraryId,
            TimeRemaining = progress.TimeRemaining,
            Items = progress
                .Items.Select(x => new LibrarySyncProgressItemDTO
                {
                    MediaType = x.MediaType,
                    Received = x.Received,
                    Total = x.Total,
                    TimeRemaining = x.TimeRemaining,
                })
                .ToList(),
            Errors = progress.Errors,
        };

        await _progressHubService.SendLibraryProgressUpdateAsync(dto, cancellationToken);
    }
}
