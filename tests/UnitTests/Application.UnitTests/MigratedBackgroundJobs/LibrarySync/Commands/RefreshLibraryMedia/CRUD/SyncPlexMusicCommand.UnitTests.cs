namespace Reaparr.Application.UnitTests;

public class SyncPlexMusicCommandUnitTests : BaseCommandUnitTest<SyncPlexMusicCommand>
{
    [Test]
    [Arguments("initial")]
    [Arguments("unchanged")]
    [Arguments("update")]
    [Arguments("album-move")]
    [Arguments("track-move")]
    [Arguments("force")]
    [Arguments("empty")]
    public async Task ShouldReconcileMusicHierarchy_WithScopedOriginalsCountsAndReports(string scenario)
    {
        // Arrange
        await SetupDatabase(
            625203,
            x =>
            {
                x.PlexServerCount = 2;
                x.PlexMusicLibraryCount = 2;
                x.MusicArtistCount = 2;
                x.MusicAlbumCount = 1;
                x.MusicTrackCount = 2;
            }
        );
        var db = IDbContext;
        var library = await db.PlexLibraries.OrderBy(x => x.Id).FirstAsync(CancellationToken);
        var artists = await db
            .PlexMusicArtists.Where(x => x.PlexLibraryId == library.Id)
            .Include(x => x.Albums)
                .ThenInclude(x => x.Tracks)
                    .ThenInclude(x => x.MediaDataList)
            .OrderBy(x => x.Id)
            .ToListAsync(CancellationToken);
        var originalAlbums = artists.SelectMany(x => x.Albums).OrderBy(x => x.Id).ToList();
        var originalTracks = originalAlbums.SelectMany(x => x.Tracks).OrderBy(x => x.Id).ToList();
        var originalArtistIds = artists.Select(x => x.Id).ToArray();
        var originalAlbumIds = originalAlbums.Select(x => x.Id).ToArray();
        var originalTrackIds = originalTracks.Select(x => x.Id).ToArray();
        var beforeData = await db
            .PlexMusicTrackData.Where(x => x.PlexLibraryId == library.Id)
            .OrderBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.PlexTrackId,
                x.PlexApiPartId,
                x.Size,
                x.OriginalFilename,
            })
            .ToListAsync(CancellationToken);
        var control = await db
            .PlexMusicTracks.Where(x => x.PlexLibraryId != library.Id)
            .OrderBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.PlexAlbumId,
                x.PlexApiRatingKey,
                x.PlexLibraryId,
                x.PlexServerId,
                x.MediaSize,
            })
            .ToListAsync(CancellationToken);
        var controlData = await db
            .PlexMusicTrackData.Where(x => x.PlexLibraryId != library.Id)
            .OrderBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.PlexTrackId,
                x.PlexApiPartId,
                x.Size,
            })
            .ToListAsync(CancellationToken);
        artists.Count.ShouldBe(2);
        originalAlbums.Count.ShouldBe(2);
        originalTracks.Count.ShouldBe(4);
        beforeData.Count.ShouldBe(4);
        long addedSize = 0;
        if (scenario is "initial" or "force")
        {
            var originals = FakeData.GetPlexMusicTrackMediaData(new Seed(625213)).Generate(2);
            originals[0]
                .UpdateInitProperty(
                    nameof(BasePlexMediaData.PlexApiMediaId),
                    originalTracks[0].MediaDataList.Single().PlexApiMediaId
                );
            originals[0].UpdateInitProperty(nameof(PlexMusicTrackMediaData.PartIndex), 1);
            originalTracks[0].MediaDataList.AddRange(originals);
            addedSize = originals.Sum(x => x.Size);
            originalTracks[0].MediaSize += addedSize;
        }
        if (scenario == "initial")
            await db.PlexMusicArtists.Where(x => x.PlexLibraryId == library.Id).ExecuteDeleteAsync(CancellationToken);
        if (scenario == "unchanged")
        {
            originalTracks[0].MediaSize = 1;
            originalTracks[0].MediaDataList.Single().UpdateInitProperty(nameof(BasePlexMediaData.Size), 1L);
        }
        if (scenario == "update")
        {
            artists.RemoveAt(1);
            artists[0].UpdatedAt = artists[0].UpdatedAt?.AddDays(1) ?? DateTime.UnixEpoch;
            originalAlbums[0].UpdatedAt = originalAlbums[0].UpdatedAt?.AddDays(1) ?? DateTime.UnixEpoch;
            originalAlbums[0].Tracks.Remove(originalTracks[1]);
            originalTracks[0].UpdatedAt = originalTracks[0].UpdatedAt?.AddDays(1) ?? DateTime.UnixEpoch;
            originalTracks[0].Title = "Updated track";
            originalTracks[0].MediaSize = 123;
            originalTracks[0].MediaDataList.Single().UpdateInitProperty(nameof(BasePlexMediaData.Size), 123L);
        }
        if (scenario == "album-move")
        {
            artists[1].Albums.Add(artists[0].Albums.Single());
            artists.RemoveAt(0);
        }
        if (scenario == "track-move")
        {
            originalAlbums[1].Tracks.AddRange(originalAlbums[0].Tracks);
            artists[0].Albums.Clear();
        }
        if (scenario == "empty")
            artists.Clear();
        library.Music.AddRange(artists);

        // Act
        var result = await TestHandlerExecuteAsync<CrudMusicReport>(
            new SyncPlexMusicCommand(new InsertMediaMetaDataCommandResponse(library), scenario == "force")
        );

        // Assert
        result.IsSuccess.ShouldBeTrue(result.ToString());
        result.Errors.Count.ShouldBe(0);
        var expectedReport = scenario switch
        {
            "initial" => new CrudMusicReport
            {
                CreatedArtists = 2,
                CreatedAlbums = 2,
                CreatedTracks = 4,
            },
            "force" => new CrudMusicReport
            {
                CreatedArtists = 2,
                CreatedAlbums = 2,
                CreatedTracks = 4,
                DeletedArtists = 2,
                DeletedAlbums = 2,
                DeletedTracks = 4,
            },
            "update" => new CrudMusicReport
            {
                UpdatedArtists = 1,
                DeletedArtists = 1,
                UpdatedAlbums = 1,
                DeletedAlbums = 1,
                UpdatedTracks = 1,
                DeletedTracks = 3,
            },
            "album-move" => new CrudMusicReport
            {
                DeletedArtists = 1,
                UnchangedArtists = 1,
                UpdatedAlbums = 1,
                UnchangedAlbums = 1,
                UnchangedTracks = 4,
            },
            "track-move" => new CrudMusicReport
            {
                UnchangedArtists = 2,
                DeletedAlbums = 1,
                UnchangedAlbums = 1,
                UpdatedTracks = 2,
                UnchangedTracks = 2,
            },
            "empty" => new CrudMusicReport
            {
                DeletedArtists = 2,
                DeletedAlbums = 2,
                DeletedTracks = 4,
            },
            _ => new CrudMusicReport
            {
                UnchangedArtists = 2,
                UnchangedAlbums = 2,
                UnchangedTracks = 4,
            },
        };
        result.Value.ShouldBe(expectedReport);
        var albums = artists.SelectMany(x => x.Albums).ToList();
        var tracks = albums.SelectMany(x => x.Tracks).ToList();
        var afterArtists = await db
            .PlexMusicArtists.Where(x => x.PlexLibraryId == library.Id)
            .OrderBy(x => x.PlexApiRatingKey)
            .ToListAsync(CancellationToken);
        var afterAlbums = await db
            .PlexMusicAlbums.Where(x => x.PlexLibraryId == library.Id)
            .OrderBy(x => x.PlexApiRatingKey)
            .ToListAsync(CancellationToken);
        var afterTracks = await db
            .PlexMusicTracks.Where(x => x.PlexLibraryId == library.Id)
            .OrderBy(x => x.PlexApiRatingKey)
            .ToListAsync(CancellationToken);
        afterArtists.Select(x => x.PlexApiRatingKey).ShouldBe(artists.Select(x => x.PlexApiRatingKey).Order());
        afterAlbums
            .Select(x => (x.PlexApiRatingKey, x.PlexArtistId))
            .ShouldBe(albums.OrderBy(x => x.PlexApiRatingKey).Select(x => (x.PlexApiRatingKey, x.PlexArtist!.Id)));
        afterTracks
            .Select(x => (x.PlexApiRatingKey, x.PlexAlbumId))
            .ShouldBe(tracks.OrderBy(x => x.PlexApiRatingKey).Select(x => (x.PlexApiRatingKey, x.PlexAlbum!.Id)));
        afterTracks.ShouldAllBe(x => x.PlexLibraryId == library.Id && x.PlexServerId == library.PlexServerId);
        if (scenario is "unchanged" or "album-move" or "track-move")
        {
            afterTracks.Select(x => x.Id).Order().ShouldBe(originalTrackIds.Order());
            afterAlbums
                .Select(x => x.Id)
                .Order()
                .ShouldBe(scenario == "track-move" ? originalAlbumIds.Skip(1).Order() : originalAlbumIds.Order());
            afterArtists
                .Select(x => x.Id)
                .Order()
                .ShouldBe(scenario == "album-move" ? originalArtistIds.Skip(1).Order() : originalArtistIds.Order());
        }
        if (scenario == "update")
        {
            afterArtists.Single().Id.ShouldBe(originalArtistIds[0]);
            afterAlbums.Single().Id.ShouldBe(originalAlbumIds[0]);
            afterTracks.Single().Id.ShouldBe(originalTrackIds[0]);
            afterTracks.Single().Title.ShouldBe("Updated track");
        }
        var afterData = await db
            .PlexMusicTrackData.Where(x => x.PlexLibraryId == library.Id)
            .OrderBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.PlexTrackId,
                x.PlexApiPartId,
                x.Size,
                x.OriginalFilename,
            })
            .ToListAsync(CancellationToken);
        if (scenario is "unchanged" or "album-move")
            afterData.ShouldBe(beforeData);
        else
            afterData
                .Select(x => (x.PlexTrackId, x.PlexApiPartId, x.Size, x.OriginalFilename))
                .OrderBy(x => x.PlexApiPartId)
                .ShouldBe(
                    tracks
                        .SelectMany(t =>
                            t.MediaDataList.Select(d => (t.Id, d.PlexApiPartId, d.Size, d.OriginalFilename))
                        )
                        .OrderBy(x => x.PlexApiPartId)
                );
        var metrics = await db.PlexLibraries.SingleAsync(x => x.Id == library.Id, CancellationToken);
        metrics.MusicArtistCount.ShouldBe(artists.Count);
        metrics.MusicAlbumCount.ShouldBe(albums.Count);
        metrics.MusicTrackCount.ShouldBe(tracks.Count);
        metrics.MediaSize.ShouldBe(
            scenario switch
            {
                "empty" => 0,
                "update" => 123,
                _ => beforeData.Sum(x => x.Size) + addedSize,
            }
        );
        (
            await db
                .PlexMusicTracks.Where(x => x.PlexLibraryId != library.Id)
                .OrderBy(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.PlexAlbumId,
                    x.PlexApiRatingKey,
                    x.PlexLibraryId,
                    x.PlexServerId,
                    x.MediaSize,
                })
                .ToListAsync(CancellationToken)
        ).ShouldBe(control);
        (
            await db
                .PlexMusicTrackData.Where(x => x.PlexLibraryId != library.Id)
                .OrderBy(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.PlexTrackId,
                    x.PlexApiPartId,
                    x.Size,
                })
                .ToListAsync(CancellationToken)
        ).ShouldBe(controlData);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task ShouldReplaceOrClearArtistMetadataWithoutChangingOtherLibraries_WhenArtistsAreUnchanged(
        bool clearMetadata
    )
    {
        // Arrange
        var seed = await SetupDatabase(
            625111,
            x =>
            {
                x.PlexServerCount = 1;
                x.PlexMusicLibraryCount = 2;
                x.MusicArtistCount = 2;
                x.MusicAlbumCount = 0;
                x.MusicTrackCount = 0;
            }
        );
        var db = IDbContext;
        var library = await db.PlexLibraries.OrderBy(x => x.Id).FirstAsync(CancellationToken);
        var artists = await db.PlexMusicArtists.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        artists.Count.ShouldBe(4);
        var actors = FakeData.GetPlexActors(seed).Generate(2);
        var countries = FakeData.GetPlexCountries(seed).Generate(2);
        var genres = FakeData.GetPlexGenresAudio(seed).Generate(2);
        db.PlexActors.AddRange(actors);
        db.PlexCountries.AddRange(countries);
        db.PlexGenresAudio.AddRange(genres);
        await db.SaveChangesAsync(CancellationToken);
        foreach (var artist in artists)
        {
            db.PlexMusicArtistActors.Add(new PlexMusicArtistActors(actors[0].Id, artist.PlexLibraryId, artist.Id));
            db.PlexMusicArtistCountries.Add(
                new PlexMusicArtistCountries(countries[0].Id, artist.PlexLibraryId, artist.Id)
            );
            db.PlexMusicArtistGenres.Add(new PlexMusicArtistGenres(genres[0].Id, artist.PlexLibraryId, artist.Id));
            if (artist.PlexLibraryId == library.Id)
            {
                library.Music.Add(artist);
                if (!clearMetadata)
                {
                    artist.Actors.AddRange([actors[1], actors[1]]);
                    artist.Countries.AddRange([countries[1], countries[1]]);
                    artist.Genres.AddRange([genres[1], genres[1]]);
                }
            }
        }
        await db.SaveChangesAsync(CancellationToken);
        var metadata = new InsertMediaMetaDataCommandResponse(library)
        {
            PlexActors = actors.ToDictionary(x => x.Key),
            PlexCountries = countries.ToDictionary(x => x.Key),
            PlexGenresAudio = genres.ToDictionary(x => x.Key),
        };

        // Act
        var result = await TestHandlerExecuteAsync<CrudMusicReport>(new SyncPlexMusicCommand(metadata, false));

        // Assert
        result.IsSuccess.ShouldBeTrue(result.ToString());
        result.Errors.Count.ShouldBe(0);
        result.Value.ShouldBe(new CrudMusicReport { UnchangedArtists = 2 });
        var expectedArtists = artists.Where(x => !clearMetadata || x.PlexLibraryId != library.Id).ToList();
        (await db.PlexMusicArtistActors.OrderBy(x => x.PlexMusicArtistId).ToListAsync(CancellationToken))
            .Select(x => (x.PlexLibraryId, x.PlexMusicArtistId, x.PlexActorId))
            .ShouldBe(
                expectedArtists.Select(x => (x.PlexLibraryId, x.Id, actors[x.PlexLibraryId == library.Id ? 1 : 0].Id))
            );
        (await db.PlexMusicArtistCountries.OrderBy(x => x.PlexMusicArtistId).ToListAsync(CancellationToken))
            .Select(x => (x.PlexLibraryId, x.PlexMusicArtistId, x.CountryId))
            .ShouldBe(
                expectedArtists.Select(x =>
                    (x.PlexLibraryId, x.Id, countries[x.PlexLibraryId == library.Id ? 1 : 0].Id)
                )
            );
        (await db.PlexMusicArtistGenres.OrderBy(x => x.PlexMusicArtistId).ToListAsync(CancellationToken))
            .Select(x => (x.PlexLibraryId, x.PlexMusicArtistId, x.GenresId))
            .ShouldBe(
                expectedArtists.Select(x => (x.PlexLibraryId, x.Id, genres[x.PlexLibraryId == library.Id ? 1 : 0].Id))
            );
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task ShouldReplaceOrClearAlbumCountriesWithoutChangingUnrefreshedAlbumsOrOtherLibraries_WhenAlbumTimestampChanges(
        bool clearCountries
    )
    {
        // Arrange
        await SetupDatabase(
            625261,
            config =>
            {
                config.PlexMusicLibraryCount = 2;
                config.MusicArtistCount = 1;
                config.MusicAlbumCount = 2;
                config.MusicTrackCount = 1;
            }
        );
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.OrderBy(x => x.Id).FirstAsync(CancellationToken);
        var artist = await dbContext
            .PlexMusicArtists.Where(x => x.PlexLibraryId == library.Id)
            .Include(x => x.Albums)
                .ThenInclude(x => x.Tracks)
                    .ThenInclude(x => x.MediaDataList)
            .SingleAsync(CancellationToken);
        var allAlbums = await dbContext.PlexMusicAlbums.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var allArtists = await dbContext.PlexMusicArtists.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        allAlbums.Count.ShouldBe(4);
        allArtists.Count.ShouldBe(2);
        var oldCountry = new PlexCountry { Name = "Old Album Country", Key = "Old Album Country".ToMd5Hash() };
        var firstCountry = new PlexCountry { Name = "First Album Country", Key = "First Album Country".ToMd5Hash() };
        var secondCountry = new PlexCountry { Name = "Second Album Country", Key = "Second Album Country".ToMd5Hash() };
        dbContext.PlexCountries.AddRange(oldCountry, firstCountry, secondCountry);
        await dbContext.SaveChangesAsync(CancellationToken);
        dbContext.PlexMusicAlbumCountries.AddRange(
            allAlbums.Select(x => new PlexMusicAlbumCountries(oldCountry.Id, x.PlexLibraryId, x.Id))
        );
        dbContext.PlexMusicArtistCountries.AddRange(
            allArtists.Select(x => new PlexMusicArtistCountries(oldCountry.Id, x.PlexLibraryId, x.Id))
        );
        await dbContext.SaveChangesAsync(CancellationToken);
        (await dbContext.PlexMusicAlbumCountries.OrderBy(x => x.PlexMusicAlbumId).ToListAsync(CancellationToken))
            .Select(x => (x.PlexLibraryId, x.PlexMusicAlbumId, x.CountryId))
            .ShouldBe(allAlbums.Select(x => (x.PlexLibraryId, x.Id, oldCountry.Id)));
        var artistLinks = await dbContext
            .PlexMusicArtistCountries.OrderBy(x => x.PlexMusicArtistId)
            .Select(x => new
            {
                x.PlexLibraryId,
                x.PlexMusicArtistId,
                x.CountryId,
            })
            .ToListAsync(CancellationToken);
        artistLinks
            .Select(x => (x.PlexLibraryId, x.PlexMusicArtistId, x.CountryId))
            .ShouldBe(allArtists.Select(x => (x.PlexLibraryId, x.Id, oldCountry.Id)));

        var targetAlbum = artist.Albums.OrderBy(x => x.Id).First();
        var unchangedAlbum = artist.Albums.OrderBy(x => x.Id).Last();
        artist.UpdatedAt = artist.UpdatedAt.GetValueOrDefault().AddSeconds(1);
        targetAlbum.UpdatedAt = targetAlbum.UpdatedAt.GetValueOrDefault().AddSeconds(1);
        artist.Countries.Add(oldCountry);
        unchangedAlbum.Countries.Add(firstCountry);
        if (!clearCountries)
        {
            targetAlbum.Countries.Add(firstCountry);
            targetAlbum.Countries.Add(secondCountry);
            targetAlbum.Countries.Add(firstCountry);
        }
        library.Music.Add(artist);
        var metadata = new InsertMediaMetaDataCommandResponse(library)
        {
            PlexCountries = new[] { oldCountry, firstCountry, secondCountry }.ToDictionary(x => x.Key),
        };

        // Act
        var result = await TestHandlerExecuteAsync<CrudMusicReport>(new SyncPlexMusicCommand(metadata));

        // Assert
        result.IsSuccess.ShouldBeTrue(result.ToString());
        result.Errors.Count.ShouldBe(0);
        result.Value.ShouldBe(
            new CrudMusicReport
            {
                UpdatedArtists = 1,
                UpdatedAlbums = 1,
                UnchangedAlbums = 1,
                UnchangedTracks = 2,
            }
        );
        var expectedLinks = allAlbums
            .Where(x => x.Id != targetAlbum.Id)
            .Select(x => (PlexLibraryId: x.PlexLibraryId, PlexMusicAlbumId: x.Id, CountryId: oldCountry.Id))
            .ToList();
        if (!clearCountries)
        {
            expectedLinks.Add((library.Id, targetAlbum.Id, firstCountry.Id));
            expectedLinks.Add((library.Id, targetAlbum.Id, secondCountry.Id));
        }
        (
            await dbContext
                .PlexMusicAlbumCountries.OrderBy(x => x.PlexMusicAlbumId)
                .ThenBy(x => x.CountryId)
                .ToListAsync(CancellationToken)
        )
            .Select(x => (x.PlexLibraryId, x.PlexMusicAlbumId, x.CountryId))
            .ShouldBe(expectedLinks.OrderBy(x => x.PlexMusicAlbumId).ThenBy(x => x.CountryId));
        (
            await dbContext
                .PlexMusicArtistCountries.OrderBy(x => x.PlexMusicArtistId)
                .Select(x => new
                {
                    x.PlexLibraryId,
                    x.PlexMusicArtistId,
                    x.CountryId,
                })
                .ToListAsync(CancellationToken)
        ).ShouldBe(artistLinks);
        (
            await dbContext
                .PlexCountries.Where(x => x.Id == oldCountry.Id || x.Id == firstCountry.Id || x.Id == secondCountry.Id)
                .OrderBy(x => x.Id)
                .Select(x => new { x.Id, x.Key })
                .ToListAsync(CancellationToken)
        )
            .Select(x => (x.Id, x.Key))
            .ShouldBe(new[] { oldCountry, firstCountry, secondCountry }.Select(x => (x.Id, x.Key)));
    }
}
