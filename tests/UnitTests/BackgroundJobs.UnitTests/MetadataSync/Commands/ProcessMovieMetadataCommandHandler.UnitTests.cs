using Microsoft.EntityFrameworkCore;
using Reaparr.PlexApi;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.BackgroundJobs.UnitTests;

public class ProcessMovieMetadataCommandHandlerUnitTests : BaseUnitTest<ProcessMovieMetadataCommandHandler>
{
    public ProcessMovieMetadataCommandHandlerUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldProcessMetadata_WhenPartsNeedMetadata()
    {
        // Arrange
        var seed = await SetupDatabase(
            5001,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var dbContext = IDbContext;
        var server = dbContext.PlexServers.First();
        var library = dbContext.PlexLibraries.First();
        var movie = dbContext.PlexMovies.First();
        var mediaData = dbContext.PlexMovieData.First();

        // Create a part without metadata
        var part = FakeData.GetPlexMovieMediaDataPart(seed).Generate();
        part.HasMetadata = false;
        part.PlexServerId = server.Id;
        part.PlexLibraryId = library.Id;
        part.RatingKey = 1001;
        part.PlexId = 2001;
        part.PlexMovieMediaDataId = mediaData.Id;
        part.LastSyncedAt = DateTime.UtcNow;
        part.Width = 1920;
        part.Height = 1080;
        part.VideoCodec = "h264";
        part.VideoBitrate = 5000;
        part.FrameRate = 24;
        part.Resolution = "1080p";
        part.Source = ReleaseSource.BluRay;
        part.ReleaseTitle = "Test Movie";
        part.Category = 2000;

        await dbContext.PlexMovieDataParts.AddAsync(part, CancellationToken);
        await dbContext.SaveChangesAsync(CancellationToken);

        // Create mock API response
        var metadata = FakePlexApiData.GetLibraryMediaMetadata(seed, PlexMediaType.Movie).Generate();
        metadata.RatingKey = part.RatingKey.ToString();
        var media = metadata.Media?.FirstOrDefault();
        media.ShouldNotBeNull();
        var apiPart = media.Part?.FirstOrDefault();
        apiPart.ShouldNotBeNull();
        apiPart.Id = part.PlexId;

        var libraryMediaItem = metadata.ToMediaItemDTO();

        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(
                    It.Is<GetDetailMetadataByRatingKeysCommand>(c =>
                        c.PlexServerId == server.Id && c.RatingKeys.Contains(part.RatingKey.ToString())
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok(new List<LibraryMediaItemDTO> { libraryMediaItem }));

        var command = new ProcessMovieMetadataCommand(server.Id, server.Name);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(1);

        var updatedPart = await dbContext.PlexMovieDataParts.FirstAsync(p => p.Id == part.Id, CancellationToken);
        updatedPart.HasMetadata.ShouldBeTrue();

        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<GetDetailMetadataByRatingKeysCommand>(c =>
                            c.PlexServerId == server.Id && c.RatingKeys.Contains(part.RatingKey.ToString())
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }

    [Fact]
    public async Task ShouldReturnZero_WhenNoPartsNeedMetadata()
    {
        // Arrange
        await SetupDatabase(
            5002,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var dbContext = IDbContext;
        var server = dbContext.PlexServers.First();

        // All parts already have metadata
        var command = new ProcessMovieMetadataCommand(server.Id, server.Name);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(0);

        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<GetDetailMetadataByRatingKeysCommand>(), It.IsAny<CancellationToken>()),
                Times.Never()
            );
    }

    [Fact]
    public async Task ShouldProcessInBatches_WhenMoreThanBatchSize()
    {
        // Arrange
        var seed = await SetupDatabase(
            5003,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var dbContext = IDbContext;
        var server = dbContext.PlexServers.First();
        var library = dbContext.PlexLibraries.First();
        var movie = dbContext.PlexMovies.First();
        var mediaData = dbContext.PlexMovieData.First();

        // Create 60 parts (more than batch size of 50)
        var parts = new List<PlexMovieMediaDataPart>();
        for (int i = 0; i < 60; i++)
        {
            var part = FakeData.GetPlexMovieMediaDataPart(seed).Generate();
            part.HasMetadata = false;
            part.PlexServerId = server.Id;
            part.PlexLibraryId = library.Id;
            part.RatingKey = 2000 + i;
            part.PlexId = 3000 + i;
            part.PlexMovieMediaDataId = mediaData.Id;
            part.LastSyncedAt = DateTime.UtcNow;
            part.Width = 1920;
            part.Height = 1080;
            part.VideoCodec = "h264";
            part.VideoBitrate = 5000;
            part.FrameRate = 24;
            part.Resolution = "1080p";
            part.Source = ReleaseSource.BluRay;
            part.ReleaseTitle = "Test Movie";
            part.Category = 2000;
            parts.Add(part);
        }

        await dbContext.PlexMovieDataParts.AddRangeAsync(parts, CancellationToken);
        await dbContext.SaveChangesAsync(CancellationToken);

        // Create mock API responses for both batches
        var libraryMediaItems = new List<LibraryMediaItemDTO>();
        foreach (var part in parts)
        {
            var metadata = FakePlexApiData.GetLibraryMediaMetadata(seed, PlexMediaType.Movie).Generate();
            metadata.RatingKey = part.RatingKey.ToString();
            var media = metadata.Media?.FirstOrDefault();
            media.ShouldNotBeNull();
            var apiPart = media.Part?.FirstOrDefault();
            apiPart.ShouldNotBeNull();
            apiPart.Id = part.PlexId;
            libraryMediaItems.Add(metadata.ToMediaItemDTO());
        }

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetDetailMetadataByRatingKeysCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(libraryMediaItems));

        var command = new ProcessMovieMetadataCommand(server.Id, server.Name);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(60);

        var updatedParts = await dbContext
            .PlexMovieDataParts.Where(p => parts.Select(x => x.Id).Contains(p.Id))
            .ToListAsync(CancellationToken);
        updatedParts.All(p => p.HasMetadata).ShouldBeTrue();

        // Should be called twice (50 + 10)
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<GetDetailMetadataByRatingKeysCommand>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2)
            );
    }

    [Fact]
    public async Task ShouldHandleApiFailure_WhenGetDetailMetadataFails()
    {
        // Arrange
        var seed = await SetupDatabase(
            5004,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var dbContext = IDbContext;
        var server = dbContext.PlexServers.First();
        var library = dbContext.PlexLibraries.First();
        var movie = dbContext.PlexMovies.First();
        var mediaData = dbContext.PlexMovieData.First();

        var part = FakeData.GetPlexMovieMediaDataPart(seed).Generate();
        part.HasMetadata = false;
        part.PlexServerId = server.Id;
        part.PlexLibraryId = library.Id;
        part.RatingKey = 3001;
        part.PlexId = 4001;
        part.PlexMovieMediaDataId = mediaData.Id;
        part.LastSyncedAt = DateTime.UtcNow;
        part.Width = 1920;
        part.Height = 1080;
        part.VideoCodec = "h264";
        part.VideoBitrate = 5000;
        part.FrameRate = 24;
        part.Resolution = "1080p";
        part.Source = ReleaseSource.BluRay;
        part.ReleaseTitle = "Test Movie";
        part.Category = 2000;

        await dbContext.PlexMovieDataParts.AddAsync(part, CancellationToken);
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetDetailMetadataByRatingKeysCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("API Error"));

        var command = new ProcessMovieMetadataCommand(server.Id, server.Name);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(0); // No parts processed due to API failure

        var unchangedPart = await dbContext.PlexMovieDataParts.FirstAsync(p => p.Id == part.Id, CancellationToken);
        unchangedPart.HasMetadata.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldSkipMissingRatingKeys_WhenRatingKeysNotReturned()
    {
        // Arrange
        var seed = await SetupDatabase(
            5005,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var dbContext = IDbContext;
        var server = dbContext.PlexServers.First();
        var library = dbContext.PlexLibraries.First();
        var movie = dbContext.PlexMovies.First();
        var mediaData = dbContext.PlexMovieData.First();

        var part = FakeData.GetPlexMovieMediaDataPart(seed).Generate();
        part.HasMetadata = false;
        part.PlexServerId = server.Id;
        part.PlexLibraryId = library.Id;
        part.RatingKey = 4001;
        part.PlexId = 5001;
        part.PlexMovieMediaDataId = mediaData.Id;
        part.LastSyncedAt = DateTime.UtcNow;
        part.Width = 1920;
        part.Height = 1080;
        part.VideoCodec = "h264";
        part.VideoBitrate = 5000;
        part.FrameRate = 24;
        part.Resolution = "1080p";
        part.Source = ReleaseSource.BluRay;
        part.ReleaseTitle = "Test Movie";
        part.Category = 2000;

        await dbContext.PlexMovieDataParts.AddAsync(part, CancellationToken);
        await dbContext.SaveChangesAsync(CancellationToken);

        // API returns empty list (rating key not found)
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetDetailMetadataByRatingKeysCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new List<LibraryMediaItemDTO>()));

        var command = new ProcessMovieMetadataCommand(server.Id, server.Name);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(0); // No parts processed

        var unchangedPart = await dbContext.PlexMovieDataParts.FirstAsync(p => p.Id == part.Id, CancellationToken);
        unchangedPart.HasMetadata.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldSkipParts_WhenPartDtoNotFound()
    {
        // Arrange
        var seed = await SetupDatabase(
            5006,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var dbContext = IDbContext;
        var server = dbContext.PlexServers.First();
        var library = dbContext.PlexLibraries.First();
        var movie = dbContext.PlexMovies.First();
        var mediaData = dbContext.PlexMovieData.First();

        var part = FakeData.GetPlexMovieMediaDataPart(seed).Generate();
        part.HasMetadata = false;
        part.PlexServerId = server.Id;
        part.PlexLibraryId = library.Id;
        part.RatingKey = 5001;
        part.PlexId = 6001;
        part.PlexMovieMediaDataId = mediaData.Id;
        part.LastSyncedAt = DateTime.UtcNow;
        part.Width = 1920;
        part.Height = 1080;
        part.VideoCodec = "h264";
        part.VideoBitrate = 5000;
        part.FrameRate = 24;
        part.Resolution = "1080p";
        part.Source = ReleaseSource.BluRay;
        part.ReleaseTitle = "Test Movie";
        part.Category = 2000;

        await dbContext.PlexMovieDataParts.AddAsync(part, CancellationToken);
        await dbContext.SaveChangesAsync(CancellationToken);

        // Create API response with different PlexId (part not found)
        var metadata = FakePlexApiData.GetLibraryMediaMetadata(seed, PlexMediaType.Movie).Generate();
        metadata.RatingKey = part.RatingKey.ToString();
        var media = metadata.Media?.FirstOrDefault();
        media.ShouldNotBeNull();
        var apiPart = media.Part?.FirstOrDefault();
        apiPart.ShouldNotBeNull();
        apiPart.Id = 9999; // Different PlexId

        var libraryMediaItem = metadata.ToMediaItemDTO();

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetDetailMetadataByRatingKeysCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new List<LibraryMediaItemDTO> { libraryMediaItem }));

        var command = new ProcessMovieMetadataCommand(server.Id, server.Name);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(0); // Part skipped because PlexId doesn't match

        var unchangedPart = await dbContext.PlexMovieDataParts.FirstAsync(p => p.Id == part.Id, CancellationToken);
        unchangedPart.HasMetadata.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldRespectMaxItemsPerRun()
    {
        // Arrange
        var seed = await SetupDatabase(
            5007,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var dbContext = IDbContext;
        var server = dbContext.PlexServers.First();
        var library = dbContext.PlexLibraries.First();
        var movie = dbContext.PlexMovies.First();
        var mediaData = dbContext.PlexMovieData.First();

        // Create 1200 parts (more than MAX_ITEMS_PER_RUN of 1000)
        var parts = new List<PlexMovieMediaDataPart>();
        for (int i = 0; i < 1200; i++)
        {
            var part = FakeData.GetPlexMovieMediaDataPart(seed).Generate();
            part.HasMetadata = false;
            part.PlexServerId = server.Id;
            part.PlexLibraryId = library.Id;
            part.RatingKey = 6000 + i;
            part.PlexId = 7000 + i;
            part.PlexMovieMediaDataId = mediaData.Id;
            part.LastSyncedAt = DateTime.UtcNow;
            part.Width = 1920;
            part.Height = 1080;
            part.VideoCodec = "h264";
            part.VideoBitrate = 5000;
            part.FrameRate = 24;
            part.Resolution = "1080p";
            part.Source = ReleaseSource.BluRay;
            part.ReleaseTitle = "Test Movie";
            part.Category = 2000;
            parts.Add(part);
        }

        await dbContext.PlexMovieDataParts.AddRangeAsync(parts, CancellationToken);
        await dbContext.SaveChangesAsync(CancellationToken);

        // Create mock API responses
        var libraryMediaItems = new List<LibraryMediaItemDTO>();
        for (int i = 0; i < 1000; i++) // Only process first 1000
        {
            var metadata = FakePlexApiData.GetLibraryMediaMetadata(seed, PlexMediaType.Movie).Generate();
            metadata.RatingKey = parts[i].RatingKey.ToString();
            var media = metadata.Media?.FirstOrDefault();
            media.ShouldNotBeNull();
            var apiPart = media.Part?.FirstOrDefault();
            apiPart.ShouldNotBeNull();
            apiPart.Id = parts[i].PlexId;
            libraryMediaItems.Add(metadata.ToMediaItemDTO());
        }

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetDetailMetadataByRatingKeysCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(libraryMediaItems));

        var command = new ProcessMovieMetadataCommand(server.Id, server.Name);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(1000); // Only 1000 processed due to MAX_ITEMS_PER_RUN

        // Should be called 20 times (1000 / 50 batches)
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<GetDetailMetadataByRatingKeysCommand>(), It.IsAny<CancellationToken>()),
                Times.Exactly(20)
            );
    }

    [Fact]
    public async Task ShouldUpdateHasMetadataFlag_WhenProcessingSucceeds()
    {
        // Arrange
        var seed = await SetupDatabase(
            5008,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var dbContext = IDbContext;
        var server = dbContext.PlexServers.First();
        var library = dbContext.PlexLibraries.First();
        var movie = dbContext.PlexMovies.First();
        var mediaData = dbContext.PlexMovieData.First();

        var part = FakeData.GetPlexMovieMediaDataPart(seed).Generate();
        part.HasMetadata = false;
        part.PlexServerId = server.Id;
        part.PlexLibraryId = library.Id;
        part.RatingKey = 7001;
        part.PlexId = 8001;
        part.PlexMovieMediaDataId = mediaData.Id;
        part.LastSyncedAt = DateTime.UtcNow;
        part.Width = 1920;
        part.Height = 1080;
        part.VideoCodec = "h264";
        part.VideoBitrate = 5000;
        part.FrameRate = 24;
        part.Resolution = "1080p";
        part.Source = ReleaseSource.BluRay;
        part.ReleaseTitle = "Test Movie";
        part.Category = 2000;

        await dbContext.PlexMovieDataParts.AddAsync(part, CancellationToken);
        await dbContext.SaveChangesAsync(CancellationToken);

        // Create a mock API response with stream data
        var metadata = FakePlexApiData.GetLibraryMediaMetadata(seed, PlexMediaType.Movie).Generate();
        metadata.RatingKey = part.RatingKey.ToString();
        var media = metadata.Media?.FirstOrDefault();
        media.ShouldNotBeNull();
        var apiPart = media.Part?.FirstOrDefault();
        apiPart.ShouldNotBeNull();
        apiPart.Id = part.PlexId;

        // Add stream data using Faker
        var streamFaker = new Faker<LukeHagar.PlexAPI.SDK.Models.Components.Stream>()
            .RuleFor(x => x.Id, _ => 1)
            .RuleFor(x => x.StreamType, _ => LukeHagar.PlexAPI.SDK.Models.Components.StreamType.Video)
            .RuleFor(x => x.BitDepth, _ => 10)
            .RuleFor(x => x.ColorSpace, _ => "bt2020")
            .RuleFor(x => x.ColorTrc, _ => "smpte2084")
            .RuleFor(x => x.Codec, _ => "hevc")
            .RuleFor(x => x.Channels, _ => 8)
            .RuleFor(x => x.LanguageCode, _ => "eng");
        apiPart.Stream = [streamFaker.Generate()];

        var libraryMediaItem = metadata.ToMediaItemDTO();

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetDetailMetadataByRatingKeysCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new List<LibraryMediaItemDTO> { libraryMediaItem }));

        var command = new ProcessMovieMetadataCommand(server.Id, server.Name);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(1);

        var updatedPart = await dbContext.PlexMovieDataParts.FirstAsync(p => p.Id == part.Id, CancellationToken);
        updatedPart.HasMetadata.ShouldBeTrue();
        updatedPart.BitDepth.ShouldBe(10);
        updatedPart.ColorSpace.ShouldBe("bt2020");
        updatedPart.IsHdr10.ShouldBeTrue();
        updatedPart.IsHdr.ShouldBeTrue();
    }
}
