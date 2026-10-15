using EFCore.BulkExtensions;

namespace Reaparr.Application;

public record SyncPlexMusicCommand(InsertMediaMetaDataCommandResponse LibraryMetadata, bool ForceMediaRefresh = false)
    : ICommand<Result<CrudMusicReport>>;

public class SyncPlexMusicCommandValidator : AbstractValidator<SyncPlexMusicCommand>
{
    public SyncPlexMusicCommandValidator()
    {
        RuleFor(x => x.LibraryMetadata).NotNull();
        RuleFor(x => x.LibraryMetadata.PlexLibrary).NotNull();
        RuleFor(x => x.LibraryMetadata.PlexLibraryId).GreaterThan(0);
        RuleFor(x => x.LibraryMetadata.PlexLibrary.PlexServerId).GreaterThan(0);
        RuleForEach(x => x.LibraryMetadata.PlexLibrary.Music)
            .ChildRules(artist =>
            {
                artist.RuleFor(x => x.PlexApiRatingKey).GreaterThan(0);
                artist
                    .RuleForEach(x => x.Albums)
                    .ChildRules(album =>
                    {
                        album.RuleFor(x => x.PlexApiRatingKey).GreaterThan(0);
                        album
                            .RuleForEach(x => x.Tracks)
                            .ChildRules(track => track.RuleFor(x => x.PlexApiRatingKey).GreaterThan(0));
                    });
            });
    }
}

public class SyncPlexMusicCommandHandler : ICommandHandler<SyncPlexMusicCommand, Result<CrudMusicReport>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public SyncPlexMusicCommandHandler(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<SyncPlexMusicCommandHandler>();
        _dbContext = dbContext;
    }

    public async Task<Result<CrudMusicReport>> ExecuteAsync(SyncPlexMusicCommand command, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return ResultExtensions.TaskIsCancelled(nameof(SyncPlexMusicCommand)).LogWarning();

        var library = command.LibraryMetadata.PlexLibrary;
        var incomingArtists = library.Music.ToList();
        var artists = new List<PlexMusicArtist>();
        var albums = new List<PlexMusicAlbum>();
        var tracks = new List<PlexMusicTrack>();
        var bulkConfig = new BulkConfig { BatchSize = 500 };
        var result = await _dbContext.ExecuteTransactionAsync(
            async (ctx, token) =>
            {
                var currentArtists = await ctx
                    .PlexMusicArtists.AsTracking()
                    .Where(x => x.PlexLibraryId == library.Id)
                    .Include(x => x.Albums)
                        .ThenInclude(x => x.Tracks)
                            .ThenInclude(x => x.MediaDataList)
                    .ToListAsync(token);
                var currentAlbums = currentArtists.SelectMany(x => x.Albums).ToList();
                var currentTracks = currentAlbums.SelectMany(x => x.Tracks).ToList();
                var report = new CrudMusicReport();
                if (command.ForceMediaRefresh)
                {
                    report.DeletedArtists = currentArtists.Count;
                    report.DeletedAlbums = currentAlbums.Count;
                    report.DeletedTracks = currentTracks.Count;
                    await ctx.PlexMusicArtists.Where(x => x.PlexLibraryId == library.Id).ExecuteDeleteAsync(token);
                    ctx.ClearChangeTracker();
                    currentArtists.Clear();
                    currentAlbums.Clear();
                    currentTracks.Clear();
                }

                var artistByKey = currentArtists.ToDictionary(x => x.PlexApiRatingKey);
                var albumByKey = currentAlbums.ToDictionary(x => x.PlexApiRatingKey);
                var trackByKey = currentTracks.ToDictionary(x => x.PlexApiRatingKey);
                var refreshedArtists =
                    new List<(PlexMusicArtist Incoming, PlexMusicArtist Stored, List<PlexMusicAlbum> Albums)>();
                var refreshedAlbums =
                    new List<(PlexMusicAlbum Incoming, PlexMusicAlbum Stored, List<PlexMusicTrack> Tracks)>();
                var createdTracks = new List<(PlexMusicTrack Incoming, PlexMusicTrack Stored)>();
                foreach (var incomingArtist in incomingArtists)
                {
                    artistByKey.TryGetValue(incomingArtist.PlexApiRatingKey, out var artist);
                    incomingArtist.Id = artist?.Id ?? 0;
                    if (artist is not null && incomingArtist.UpdatedAt == artist.UpdatedAt)
                    {
                        artists.Add(artist);
                        albums.AddRange(artist.Albums);
                        tracks.AddRange(artist.Albums.SelectMany(x => x.Tracks));
                        continue;
                    }

                    incomingArtist.PlexLibraryId = library.Id;
                    incomingArtist.PlexServerId = library.PlexServerId;
                    if (artist is null)
                    {
                        artist = (PlexMusicArtist)ctx.Entry(incomingArtist).CurrentValues.ToObject();
                        ctx.Entry(artist).State = EntityState.Added;
                        report.CreatedArtists++;
                    }
                    else
                    {
                        ctx.Entry(artist).CurrentValues.SetValues(incomingArtist);
                        report.UpdatedArtists++;
                    }

                    artists.Add(artist);
                    var artistAlbums = new List<PlexMusicAlbum>(incomingArtist.Albums.Count);
                    refreshedArtists.Add((incomingArtist, artist, artistAlbums));
                    foreach (var incomingAlbum in incomingArtist.Albums)
                    {
                        albumByKey.TryGetValue(incomingAlbum.PlexApiRatingKey, out var album);
                        incomingAlbum.Id = album?.Id ?? 0;
                        if (album is not null && incomingAlbum.UpdatedAt == album.UpdatedAt)
                        {
                            artistAlbums.Add(album);
                            albums.Add(album);
                            tracks.AddRange(album.Tracks);
                            continue;
                        }

                        incomingAlbum.PlexArtist = incomingArtist;
                        incomingAlbum.PlexArtistId = artist.Id;
                        incomingAlbum.ParentKey = artist.PlexApiRatingKey;
                        incomingAlbum.PlexLibraryId = library.Id;
                        incomingAlbum.PlexServerId = library.PlexServerId;
                        if (album is null)
                        {
                            album = (PlexMusicAlbum)ctx.Entry(incomingAlbum).CurrentValues.ToObject();
                            album.PlexArtist = artist;
                            ctx.Entry(album).State = EntityState.Added;
                            report.CreatedAlbums++;
                        }
                        else
                        {
                            ctx.Entry(album).CurrentValues.SetValues(incomingAlbum);
                            album.PlexArtist = artist;
                            report.UpdatedAlbums++;
                        }

                        artistAlbums.Add(album);
                        albums.Add(album);
                        var albumTracks = new List<PlexMusicTrack>(incomingAlbum.Tracks.Count);
                        refreshedAlbums.Add((incomingAlbum, album, albumTracks));
                        foreach (var incomingTrack in incomingAlbum.Tracks)
                        {
                            trackByKey.TryGetValue(incomingTrack.PlexApiRatingKey, out var track);
                            incomingTrack.Id = track?.Id ?? 0;
                            if (track is not null && incomingTrack.UpdatedAt == track.UpdatedAt)
                            {
                                albumTracks.Add(track);
                                tracks.Add(track);
                                continue;
                            }

                            incomingTrack.PlexAlbum = incomingAlbum;
                            incomingTrack.PlexAlbumId = album.Id;
                            incomingTrack.ParentKey = album.PlexApiRatingKey;
                            incomingTrack.PlexLibraryId = library.Id;
                            incomingTrack.PlexServerId = library.PlexServerId;
                            if (track is null)
                            {
                                track = (PlexMusicTrack)ctx.Entry(incomingTrack).CurrentValues.ToObject();
                                track.PlexAlbum = album;
                                ctx.Entry(track).State = EntityState.Added;
                                createdTracks.Add((incomingTrack, track));
                                report.CreatedTracks++;
                            }
                            else
                            {
                                ctx.Entry(track).CurrentValues.SetValues(incomingTrack);
                                track.PlexAlbum = album;
                                report.UpdatedTracks++;
                            }

                            ReconcileTrackMedia(ctx, incomingTrack, track, library.Id, library.PlexServerId);
                            albumTracks.Add(track);
                            tracks.Add(track);
                        }

                        album.ChildCount = albumTracks.Count;
                        album.TrackCount = albumTracks.Count;
                        album.Duration = albumTracks.Sum(x => x.Duration);
                        album.MediaSize = albumTracks.Sum(x => x.MediaSize);
                    }

                    artist.ChildCount = artistAlbums.Count;
                    artist.Duration = artistAlbums.Sum(x => x.Duration);
                    artist.MediaSize = artistAlbums.Sum(x => x.MediaSize);
                }

                // Resolve moves before severing required relationships, so EF does not delete moved children as orphans.
                foreach (var (_, stored, children) in refreshedArtists)
                    stored.Albums = children;
                foreach (var (_, stored, children) in refreshedAlbums)
                    stored.Tracks = children;

                var artistKeys = artists.Select(x => x.PlexApiRatingKey).ToHashSet();
                var albumKeys = albums.Select(x => x.PlexApiRatingKey).ToHashSet();
                var trackKeys = tracks.Select(x => x.PlexApiRatingKey).ToHashSet();
                var deletedTracks = currentTracks.Where(x => !trackKeys.Contains(x.PlexApiRatingKey)).ToList();
                var deletedAlbums = currentAlbums.Where(x => !albumKeys.Contains(x.PlexApiRatingKey)).ToList();
                var deletedArtists = currentArtists.Where(x => !artistKeys.Contains(x.PlexApiRatingKey)).ToList();
                ctx.PlexMusicTracks.RemoveRange(deletedTracks);
                ctx.PlexMusicAlbums.RemoveRange(deletedAlbums);
                ctx.PlexMusicArtists.RemoveRange(deletedArtists);
                report.DeletedTracks += deletedTracks.Count;
                report.DeletedAlbums += deletedAlbums.Count;
                report.DeletedArtists += deletedArtists.Count;
                report.UnchangedArtists = artists.Count - report.CreatedArtists - report.UpdatedArtists;
                report.UnchangedAlbums = albums.Count - report.CreatedAlbums - report.UpdatedAlbums;
                report.UnchangedTracks = tracks.Count - report.CreatedTracks - report.UpdatedTracks;
                await ctx.SaveChangesAsync(token);

                foreach (var (incoming, stored, _) in refreshedArtists)
                    incoming.Id = stored.Id;
                foreach (var (incoming, stored, _) in refreshedAlbums)
                {
                    incoming.Id = stored.Id;
                    incoming.PlexArtistId = stored.PlexArtistId;
                }

                foreach (var (incoming, stored) in createdTracks)
                {
                    incoming.Id = stored.Id;
                    incoming.PlexAlbumId = stored.PlexAlbumId;
                }

                if (refreshedArtists.Count > 0)
                {
                    var metadataArtists = refreshedArtists.Select(x => x.Incoming).ToList();
                    await SyncArtistCountries(ctx, metadataArtists, command.LibraryMetadata, bulkConfig, token);
                    await SyncArtistGenres(ctx, metadataArtists, command.LibraryMetadata, bulkConfig, token);
                    await SyncArtistActors(ctx, metadataArtists, command.LibraryMetadata, bulkConfig, token);
                    await SyncAlbumCountries(
                        ctx,
                        refreshedAlbums.Select(x => x.Incoming).ToList(),
                        command.LibraryMetadata,
                        bulkConfig,
                        token
                    );
                    await PersistMusicAudioQualities(
                        ctx,
                        refreshedArtists.Select(x => x.Stored).ToList(),
                        refreshedAlbums.Select(x => x.Stored).ToList(),
                        library.Id,
                        bulkConfig,
                        token
                    );
                }

                await ctx.SetMusicMediaMetrics(
                    library.Id,
                    artists.Count,
                    albums.Count,
                    tracks.Count,
                    tracks.Sum(x => x.MediaSize),
                    token
                );
                if (report.ChangedItemCount > 0)
                    await ctx
                        .PlexLibraries.Where(x => x.Id == library.Id)
                        .ExecuteUpdateAsync(x => x.SetProperty(y => y.Outdated, true), token);
                return report;
            },
            ct
        );
        if (!result.IsSuccess)
            return result.LogIfFailed();

        library.Music.AddRange(artists);
        library.Albums.AddRange(albums);
        library.Tracks.AddRange(tracks);
        _log.Here().Information("Synchronized Music library {PlexLibraryId}: {@Report}", library.Id, result.Value);
        return result;
    }

    private static void ReconcileTrackMedia(
        IReaparrDbContext ctx,
        PlexMusicTrack incoming,
        PlexMusicTrack track,
        int libraryId,
        int serverId
    )
    {
        var remainingRows = track.MediaDataList.ToDictionary(DataKey);
        var seenKeys = new HashSet<(int MediaId, int PartId)>();
        var mediaRows = new List<PlexMusicTrackMediaData>(incoming.MediaDataList.Count);
        foreach (var row in incoming.MediaDataList)
        {
            var key = DataKey(row);
            if (!seenKeys.Add(key))
                continue;

            row.PlexTrackId = track.Id;
            row.PlexLibraryId = libraryId;
            row.PlexServerId = serverId;
            row.AudioQuality = row.ClassifyAudioQuality();
            if (remainingRows.Remove(key, out var stored))
            {
                row.Id = stored.Id;
                row.GeneratedFilename = stored.GeneratedFilename;
                row.GeneratedNameSyncedAt = stored.GeneratedNameSyncedAt;
                row.NeedsGeneratedName = stored.NeedsGeneratedName;
                ctx.Entry(stored).CurrentValues.SetValues(row);
            }
            else
            {
                row.Id = 0;
                stored = row;
                stored.PlexTrack = track;
                ctx.Entry(stored).State = EntityState.Added;
            }

            mediaRows.Add(stored);
        }

        ctx.PlexMusicTrackData.RemoveRange(remainingRows.Values);
        track.MediaDataList = mediaRows;
        track.MediaSize = mediaRows.Sum(x => x.Size);
    }

    private static async Task PersistMusicAudioQualities(
        IReaparrDbContext ctx,
        List<PlexMusicArtist> artists,
        List<PlexMusicAlbum> albums,
        int libraryId,
        BulkConfig bulkConfig,
        CancellationToken ct
    )
    {
        var albumQualityKeys = new HashSet<(int AlbumId, AudioQuality AudioQuality)>();
        foreach (var album in albums)
        foreach (var track in album.Tracks)
        foreach (var data in track.MediaDataList)
            albumQualityKeys.Add((album.Id, data.AudioQuality));
        var artistQualityKeys = new HashSet<(int ArtistId, AudioQuality AudioQuality)>();
        foreach (var artist in artists)
        foreach (var album in artist.Albums)
        foreach (var track in album.Tracks)
        foreach (var data in track.MediaDataList)
            artistQualityKeys.Add((artist.Id, data.AudioQuality));
        var albumIds = albums.Select(x => x.Id).ToList();
        var artistIds = artists.Select(x => x.Id).ToList();

        var existingAlbumQualities = await ctx
            .PlexMusicAlbumMediaQualities.Where(x =>
                x.PlexLibraryId == libraryId && albumIds.Contains(x.PlexMusicAlbumId)
            )
            .Select(x => new
            {
                x.Id,
                x.PlexMusicAlbumId,
                Quality = x.AudioQuality,
            })
            .ToListAsync(ct);
        var obsoleteAlbumQualityIds = new List<int>();
        foreach (var quality in existingAlbumQualities)
            if (!albumQualityKeys.Remove((quality.PlexMusicAlbumId, quality.Quality)))
                obsoleteAlbumQualityIds.Add(quality.Id);
        if (obsoleteAlbumQualityIds.Count > 0)
            await ctx
                .PlexMusicAlbumMediaQualities.Where(x => obsoleteAlbumQualityIds.Contains(x.Id))
                .ExecuteDeleteAsync(ct);
        if (albumQualityKeys.Count > 0)
            await ctx.BulkInsertAsync(
                albumQualityKeys
                    .Select(quality => new PlexMusicAlbumMediaQuality
                    {
                        PlexLibraryId = libraryId,
                        PlexMusicAlbumId = quality.AlbumId,
                        AudioQuality = quality.AudioQuality,
                    })
                    .ToList(),
                bulkConfig,
                ct
            );

        var existingArtistQualities = await ctx
            .PlexMusicArtistMediaQualities.Where(x =>
                x.PlexLibraryId == libraryId && artistIds.Contains(x.PlexMusicArtistId)
            )
            .Select(x => new
            {
                x.Id,
                x.PlexMusicArtistId,
                x.AudioQuality,
            })
            .ToListAsync(ct);
        var obsoleteArtistQualityIds = new List<int>();
        foreach (var quality in existingArtistQualities)
            if (!artistQualityKeys.Remove((quality.PlexMusicArtistId, quality.AudioQuality)))
                obsoleteArtistQualityIds.Add(quality.Id);
        if (obsoleteArtistQualityIds.Count > 0)
            await ctx
                .PlexMusicArtistMediaQualities.Where(x => obsoleteArtistQualityIds.Contains(x.Id))
                .ExecuteDeleteAsync(ct);
        if (artistQualityKeys.Count > 0)
            await ctx.BulkInsertAsync(
                artistQualityKeys
                    .Select(quality => new PlexMusicArtistMediaQuality
                    {
                        PlexLibraryId = libraryId,
                        PlexMusicArtistId = quality.ArtistId,
                        AudioQuality = quality.AudioQuality,
                    })
                    .ToList(),
                bulkConfig,
                ct
            );
    }

    private static async Task SyncArtistActors(
        IReaparrDbContext ctx,
        List<PlexMusicArtist> artists,
        InsertMediaMetaDataCommandResponse metadata,
        BulkConfig bulkConfig,
        CancellationToken ct
    )
    {
        var rows = new List<PlexMusicArtistActors>();
        var keys = new HashSet<(int ActorId, int ArtistId)>();
        foreach (var artist in artists)
        foreach (var actor in artist.Actors)
            if (metadata.PlexActors.TryGetValue(actor.Key, out var stored) && keys.Add((stored.Id, artist.Id)))
                rows.Add(new PlexMusicArtistActors(stored.Id, metadata.PlexLibraryId, artist.Id));

        var artistIds = artists.Select(x => x.Id).ToList();
        await ctx
            .PlexMusicArtistActors.Where(x =>
                x.PlexLibraryId == metadata.PlexLibraryId && artistIds.Contains(x.PlexMusicArtistId)
            )
            .ExecuteDeleteAsync(ct);
        if (rows.Count > 0)
            await ctx.BulkInsertAsync(rows, bulkConfig, ct);
    }

    private static async Task SyncArtistGenres(
        IReaparrDbContext ctx,
        List<PlexMusicArtist> artists,
        InsertMediaMetaDataCommandResponse metadata,
        BulkConfig bulkConfig,
        CancellationToken ct
    )
    {
        var rows = new List<PlexMusicArtistGenres>();
        var keys = new HashSet<(int GenreId, int ArtistId)>();
        foreach (var artist in artists)
        foreach (var genre in artist.Genres)
            if (metadata.PlexGenresAudio.TryGetValue(genre.Key, out var stored) && keys.Add((stored.Id, artist.Id)))
                rows.Add(new PlexMusicArtistGenres(stored.Id, metadata.PlexLibraryId, artist.Id));

        var artistIds = artists.Select(x => x.Id).ToList();
        await ctx
            .PlexMusicArtistGenres.Where(x =>
                x.PlexLibraryId == metadata.PlexLibraryId && artistIds.Contains(x.PlexMusicArtistId)
            )
            .ExecuteDeleteAsync(ct);
        if (rows.Count > 0)
            await ctx.BulkInsertAsync(rows, bulkConfig, ct);
    }

    private static async Task SyncArtistCountries(
        IReaparrDbContext ctx,
        List<PlexMusicArtist> artists,
        InsertMediaMetaDataCommandResponse metadata,
        BulkConfig bulkConfig,
        CancellationToken ct
    )
    {
        var rows = new List<PlexMusicArtistCountries>();
        var keys = new HashSet<(int CountryId, int ArtistId)>();
        foreach (var artist in artists)
        foreach (var country in artist.Countries)
            if (metadata.PlexCountries.TryGetValue(country.Key, out var stored) && keys.Add((stored.Id, artist.Id)))
                rows.Add(new PlexMusicArtistCountries(stored.Id, metadata.PlexLibraryId, artist.Id));

        var artistIds = artists.Select(x => x.Id).ToList();
        await ctx
            .PlexMusicArtistCountries.Where(x =>
                x.PlexLibraryId == metadata.PlexLibraryId && artistIds.Contains(x.PlexMusicArtistId)
            )
            .ExecuteDeleteAsync(ct);
        if (rows.Count > 0)
            await ctx.BulkInsertAsync(rows, bulkConfig, ct);
    }

    private static async Task SyncAlbumCountries(
        IReaparrDbContext ctx,
        List<PlexMusicAlbum> albums,
        InsertMediaMetaDataCommandResponse metadata,
        BulkConfig bulkConfig,
        CancellationToken ct
    )
    {
        var rows = new List<PlexMusicAlbumCountries>();
        var keys = new HashSet<(int CountryId, int AlbumId)>();
        foreach (var album in albums)
        foreach (var country in album.Countries)
            if (metadata.PlexCountries.TryGetValue(country.Key, out var stored) && keys.Add((stored.Id, album.Id)))
                rows.Add(new PlexMusicAlbumCountries(stored.Id, metadata.PlexLibraryId, album.Id));

        var albumIds = albums.Select(x => x.Id).ToList();
        await ctx
            .PlexMusicAlbumCountries.Where(x =>
                x.PlexLibraryId == metadata.PlexLibraryId && albumIds.Contains(x.PlexMusicAlbumId)
            )
            .ExecuteDeleteAsync(ct);
        if (rows.Count > 0)
            await ctx.BulkInsertAsync(rows, bulkConfig, ct);
    }

    private static (int MediaId, int PartId) DataKey(PlexMusicTrackMediaData data) =>
        (data.PlexApiMediaId, data.PlexApiPartId);
}

public record CrudMusicReport
{
    public int CreatedArtists { get; set; }
    public int UpdatedArtists { get; set; }
    public int DeletedArtists { get; set; }
    public int UnchangedArtists { get; set; }
    public int CreatedAlbums { get; set; }
    public int UpdatedAlbums { get; set; }
    public int DeletedAlbums { get; set; }
    public int UnchangedAlbums { get; set; }
    public int CreatedTracks { get; set; }
    public int UpdatedTracks { get; set; }
    public int DeletedTracks { get; set; }
    public int UnchangedTracks { get; set; }

    public int ChangedItemCount =>
        CreatedArtists
        + UpdatedArtists
        + DeletedArtists
        + CreatedAlbums
        + UpdatedAlbums
        + DeletedAlbums
        + CreatedTracks
        + UpdatedTracks
        + DeletedTracks;
}
