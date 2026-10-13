namespace Reaparr.PlexApi.UnitTests;

public class PlexMediaDataMapperUnitTests : BaseUnitTest
{
    [Test]
    public void ShouldPreservePlexLabelsWhenMappingMusicArtistGenres()
    {
        // Arrange
        const string genreName = "Documentary / Sport";
        const string genreKey = "plex-audio-documentary-sport";
        var source = FakeData
            .GetLibraryMediaItemDTO(new Seed(9200), mediaType: PlexMediaType.MusicArtist)
            .Generate() with
        {
            Genre =
            [
                new LibraryMediaItemGenreDTO
                {
                    PlexId = 42,
                    Name = genreName,
                    Filter = "genre=42",
                    Key = genreKey,
                },
            ],
        };

        // Act
        var result = source.ToPlexMusicArtist();

        // Assert
        var genre = result.Genres.ShouldHaveSingleItem();
        (genre.Name, genre.Key).ShouldBe((genreName, genreKey));
    }

    [Test]
    public void ShouldMapCanonicalNativeIdentity_WhenPlexGuidUsesEntityNamespace()
    {
        // Arrange
        var source = FakeData
            .GetLibraryMediaItemDTO(new Seed(9200), mediaType: PlexMediaType.MusicArtist)
            .Generate() with
        {
            Guid = "PLEX://ARTIST/5D07BCE8403C64029056B5A6",
            Guids = [new MetaDataGuidsDTO("mbid://CC2C9C3C-B7BC-4B8B-84D8-4FBD8779E493")],
        };

        // Act
        var result = source.ToPlexMusicArtist();

        // Assert
        result.PlexApiIdentityId.ShouldBe("5d07bce8403c64029056b5a6");
        result.MusicBrainzArtistId.ShouldBe("cc2c9c3c-b7bc-4b8b-84d8-4fbd8779e493");
        result.Guid.ShouldBe(string.Empty);
    }

    [Test]
    [Arguments("")]
    [Arguments("local://artist/5d07bce8403c64029056b5a6")]
    [Arguments("mbid://5d07bce8403c64029056b5a6")]
    [Arguments("plex://album/5d07bce8403c64029056b5a6")]
    [Arguments("plex://artist/")]
    public void ShouldLeaveNativeIdentityNull_WhenPlexGuidIsMissingMalformedOrWrongNamespace(string guid)
    {
        // Arrange
        var source = FakeData
            .GetLibraryMediaItemDTO(new Seed(9202), mediaType: PlexMediaType.MusicArtist)
            .Generate() with
        {
            Guid = guid,
            Guids = [],
        };

        // Act
        var result = source.ToPlexMusicArtist();

        // Assert
        result.PlexApiIdentityId.ShouldBeNull();
    }

    [Test]
    [Arguments(PlexMediaType.MusicAlbum, "plex://artist/5d07bce8403c64029056b5a6")]
    [Arguments(PlexMediaType.MusicTrack, "")]
    public void ShouldLeaveAlbumAndTrackNativeIdentityNull_WhenPlexGuidIsWrongNamespaceOrAbsent(
        PlexMediaType mediaType,
        string guid
    )
    {
        // Arrange
        var source = FakeData
            .GetLibraryMediaItemDTO(new Seed(9204), mediaType: mediaType)
            .RuleFor(
                x => x.Title,
                mediaType == PlexMediaType.MusicAlbum ? "Unsupported album identity" : "Unsupported track identity"
            )
            .RuleFor(x => x.Guid, guid)
            .Generate() with
        {
            Guids = [],
        };

        // Act
        var identity = mediaType switch
        {
            PlexMediaType.MusicAlbum => source.ToPlexMusicAlbum().PlexApiIdentityId,
            PlexMediaType.MusicTrack => source.ToPlexMusicTrack().PlexApiIdentityId,
            _ => throw new ArgumentOutOfRangeException(nameof(mediaType), mediaType, null),
        };

        // Assert
        identity.ShouldBeNull();
    }

    [Test]
    public void ShouldPreferPrimaryMusicBrainzId_WhenPrimaryAndSupplementaryIdsAreValid()
    {
        // Arrange
        var primaryId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var supplementaryId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var source = FakeData
            .GetLibraryMediaItemDTO(new Seed(9201), mediaType: PlexMediaType.MusicArtist)
            .Generate() with
        {
            Guid = $"mbid://{primaryId}",
            Guids = [new MetaDataGuidsDTO($"mbid://{supplementaryId}")],
        };

        // Act
        var result = source.ToPlexMusicArtist();

        // Assert
        result.MusicBrainzArtistId.ShouldBe(primaryId.ToString());
    }

    [Test]
    public void ShouldUseFirstValidSupplementaryMusicBrainzId_WhenPrimaryAndEarlierEntriesAreInvalid()
    {
        // Arrange
        var expectedId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var source = FakeData
            .GetLibraryMediaItemDTO(new Seed(9203), mediaType: PlexMediaType.MusicArtist)
            .Generate() with
        {
            Guid = "plex://artist/primary",
            Guids =
            [
                new MetaDataGuidsDTO("mbid://not-a-uuid"),
                new MetaDataGuidsDTO("plex://artist/supplementary"),
                new MetaDataGuidsDTO($"mbid://{expectedId}"),
            ],
        };

        // Act
        var result = source.ToPlexMusicArtist();

        // Assert
        result.MusicBrainzArtistId.ShouldBe(expectedId.ToString());
    }

    [Test]
    [Arguments(float.NaN)]
    [Arguments(float.PositiveInfinity)]
    [Arguments(float.MaxValue)]
    public void ShouldMapEachPartVideoFrameRateFromItsVideoStream_WithNumericMediaFallback(float unusableRate)
    {
        // Arrange
        var seed = new Seed(9205);
        var videoStream = FakeData.GetLibraryMediaItemStreamDTO(seed, StreamType.Video).Generate() with
        {
            FrameRate = 23.976f,
        };
        var ignoredAudioStream = FakeData.GetLibraryMediaItemStreamDTO(seed, StreamType.Audio).Generate() with
        {
            FrameRate = 120f,
        };
        var firstPart = FakeData.GetLibraryMediaItemPartDTO(seed).Generate() with
        {
            Id = 101,
            Stream = [ignoredAudioStream, videoStream],
        };
        var unusableVideoStream = videoStream with { FrameRate = unusableRate };
        var secondPart = FakeData.GetLibraryMediaItemPartDTO(seed).Generate() with
        {
            Id = 102,
            Stream = [ignoredAudioStream, unusableVideoStream],
        };
        var media = FakeData.GetLibraryMediaItemMediaDTO(seed).Generate() with
        {
            VideoFrameRate = "25.5",
            Parts = [firstPart, secondPart],
        };
        var source = FakeData.GetLibraryMediaItemDTO(seed, mediaType: PlexMediaType.OtherVideos).Generate() with
        {
            Media = [media],
        };

        // Act
        var result = source.ToPlexOtherVideo();

        // Assert
        result
            .MediaDataList.OrderBy(x => x.PartIndex)
            .Select(x => (x.PlexApiPartId, x.VideoFrameRate))
            .ShouldBe([(101, (decimal?)23.976m), (102, (decimal?)25.5m)]);
    }

    [Test]
    public void ShouldMapSelectedAudioStreamPerPartAndIgnoreLyrics()
    {
        // Arrange
        var seed = new Seed(9207);
        var lyrics = FakeData.GetLibraryMediaItemStreamDTO(seed, StreamType.Subtitle).Generate() with
        {
            Id = 1,
            Selected = true,
            Default = true,
            Codec = "lrc",
            Bitrate = 9999,
        };
        var defaultAudio = FakeData.GetLibraryMediaItemStreamDTO(seed, StreamType.Audio).Generate() with
        {
            Id = 2,
            Index = 1,
            Selected = false,
            Default = true,
            Codec = "mp3",
            Bitrate = 128,
            BitDepth = null,
            SamplingRate = 44100,
            Channels = 2,
            AudioChannelLayout = "stereo",
            Profile = null,
        };
        var selectedAudio = defaultAudio with
        {
            Id = 3,
            Index = 2,
            Selected = true,
            Default = false,
            Codec = "flac",
            Bitrate = 737,
            BitDepth = 16,
            Profile = "lossless",
        };
        var part = FakeData.GetLibraryMediaItemPartDTO(seed).Generate() with
        {
            Id = 101,
            Stream = [lyrics, defaultAudio, selectedAudio],
        };
        var media = FakeData.GetLibraryMediaItemMediaDTO(seed).Generate() with
        {
            Id = 201,
            AudioCodec = "aac",
            Bitrate = 256,
            AudioChannels = 6,
            AudioProfile = "lc",
            Parts = [part],
        };
        var source = FakeData
            .GetLibraryMediaItemDTO(seed, mediaType: PlexMediaType.MusicTrack)
            .RuleFor(x => x.Title, "Selected audio track")
            .RuleFor(x => x.Guid, "local://track/9207")
            .Generate() with
        {
            Media = [media],
        };

        // Act
        var result = source.ToPlexMusicTrack();

        // Assert
        result.Quality.ShouldBe(VideoQuality.None);
        var file = result.MediaDataList.ShouldHaveSingleItem();
        file.VideoResolution.ShouldBe(VideoQuality.None);
        file.Quality.ShouldBe(VideoQuality.None);
        file.AudioCodec.ShouldBe("flac");
        file.AudioBitrateKbps.ShouldBe(737);
        file.AudioBitDepth.ShouldBe(16);
        file.AudioSampleRateHz.ShouldBe(44100);
        file.AudioChannels.ShouldBe(2);
        file.AudioChannelLayout.ShouldBe("stereo");
        file.AudioProfile.ShouldBe("lossless");
        file.AudioQuality.ShouldNotBe(AudioQuality.Unknown);
    }

    [Test]
    public void ShouldPreferDefaultThenDeterministicAudioStreamAndUseOnlySuppliedMediaFallbacks()
    {
        // Arrange
        var seed = new Seed(9208);
        var first = FakeData.GetLibraryMediaItemStreamDTO(seed, StreamType.Audio).Generate() with
        {
            Id = 20,
            Index = 3,
            Selected = null,
            Default = null,
            Codec = "vorbis",
            Bitrate = 160,
            BitDepth = null,
            SamplingRate = 44100,
            Channels = 2,
            AudioChannelLayout = "stereo",
            Profile = null,
        };
        var deterministic = first with { Id = 10, Index = 1, Bitrate = 192 };
        var partWithStreams = FakeData.GetLibraryMediaItemPartDTO(seed).Generate() with
        {
            Id = 101,
            Stream = [first, deterministic],
        };
        var partWithoutStreams = FakeData.GetLibraryMediaItemPartDTO(seed).Generate() with { Id = 102, Stream = [] };
        var media = FakeData.GetLibraryMediaItemMediaDTO(seed).Generate() with
        {
            AudioCodec = "mp3",
            Bitrate = 128,
            AudioChannels = 2,
            AudioProfile = string.Empty,
            Parts = [partWithStreams, partWithoutStreams],
        };
        var source = FakeData
            .GetLibraryMediaItemDTO(seed, mediaType: PlexMediaType.MusicTrack)
            .RuleFor(x => x.Title, "Audio fallback track")
            .RuleFor(x => x.Guid, "local://track/9208")
            .Generate() with
        {
            Media = [media],
        };

        // Act
        var result = source.ToPlexMusicTrack();

        // Assert
        var files = result.MediaDataList.OrderBy(x => x.PartIndex).ToList();
        files[0].AudioCodec.ShouldBe("vorbis");
        files[0].AudioBitrateKbps.ShouldBe(192);
        files[0].AudioSampleRateHz.ShouldBe(44100);
        files[1].AudioCodec.ShouldBe("mp3");
        files[1].AudioBitrateKbps.ShouldBe(128);
        files[1].AudioBitDepth.ShouldBeNull();
        files[1].AudioSampleRateHz.ShouldBeNull();
        files[1].AudioChannels.ShouldBe(2);
        files[1].AudioChannelLayout.ShouldBeNull();
        files[1].AudioProfile.ShouldBeNull();
    }

    [Test]
    public void ShouldPopulateOnlyRetainedEntityMusicBrainzIdentifiers()
    {
        // Arrange
        var artistId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var releaseId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var releaseTrackId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var seed = new Seed(9209);
        var artist = FakeData.GetLibraryMediaItemDTO(seed, mediaType: PlexMediaType.MusicArtist).Generate() with
        {
            Guid = "PLEX://ARTIST/5D07BCE8403C64029056B5A6",
            Guids = [new MetaDataGuidsDTO($"mbid://{artistId}")],
        };
        var album = FakeData
            .GetLibraryMediaItemDTO(seed, mediaType: PlexMediaType.MusicAlbum)
            .RuleFor(x => x.Title, "Catalog album")
            .RuleFor(x => x.Guid, "plex://album/catalog")
            .Generate() with
        {
            Guid = "PLEX://ALBUM/5D07C2D8403C640290914CF7",
            Guids = [new MetaDataGuidsDTO($"mbid://{releaseId}")],
        };
        var track = FakeData
            .GetLibraryMediaItemDTO(seed, mediaType: PlexMediaType.MusicTrack)
            .RuleFor(x => x.Title, "Catalog track")
            .RuleFor(x => x.Guid, "plex://track/catalog")
            .Generate() with
        {
            Guid = "PLEX://TRACK/5D07C9A1403C640290CA914A",
            Guids = [new MetaDataGuidsDTO($"mbid://{releaseTrackId}")],
        };

        // Act
        var mappedArtist = artist.ToPlexMusicArtist();
        var mappedAlbum = album.ToPlexMusicAlbum();
        var mappedTrack = track.ToPlexMusicTrack();

        // Assert
        mappedArtist.PlexApiIdentityId.ShouldBe("5d07bce8403c64029056b5a6");
        mappedArtist.MusicBrainzArtistId.ShouldBe(artistId.ToString());
        mappedArtist.Guid.ShouldBe(string.Empty);
        mappedAlbum.PlexApiIdentityId.ShouldBe("5d07c2d8403c640290914cf7");
        mappedAlbum.MusicBrainzReleaseId.ShouldBe(releaseId.ToString());
        mappedAlbum.Guid.ShouldBe(string.Empty);
        mappedTrack.PlexApiIdentityId.ShouldBe("5d07c9a1403c640290ca914a");
        mappedTrack.MusicBrainzReleaseTrackId.ShouldBe(releaseTrackId.ToString());
        mappedTrack.Guid.ShouldBe(string.Empty);
    }
}
