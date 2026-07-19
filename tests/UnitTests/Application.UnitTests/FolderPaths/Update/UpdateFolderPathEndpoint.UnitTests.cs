namespace Reaparr.Application.UnitTests;

public class UpdateFolderPathEndpointUnitTests : BaseEndpointUnitTest<UpdateFolderPathEndpoint, UpdateFolderPathEndpointRequest, ResultDTO<FolderPathDTO>>
{
    [Test]
    public async Task ShouldUpdateFolderPath_WhenFolderPathExists()
    {
        // Arrange
        await SetupDatabase(44201);

        var dbContext = IDbContext;
        var existingFolderPath = new FolderPath
        {
            Id = 11,
            DisplayName = "Movie destination",
            DirectoryPath = "/tmp/movies-old",
            FolderType = FolderType.MovieFolder,
            MediaType = PlexMediaType.Movie,
        };
        dbContext.FolderPaths.Add(existingFolderPath);
        await dbContext.SaveChangesNewAsync(CancellationToken);

        var request = new UpdateFolderPathEndpointRequest
        {
            FolderPathDTO = new FolderPathDTO
            {
                Id = existingFolderPath.Id,
                DisplayName = "Updated movie destination",
                Directory = "/tmp/movies-new",
                FolderType = FolderType.MovieFolder,
                MediaType = PlexMediaType.Movie,
                IsDefault = false,
                IsValid = true,
            },
        };

        // Act
        var endpointResult = await TestEndpointHandleAsync(request);
        var result = endpointResult.Response;
        var updatedFolderPath = await dbContext.FolderPaths.SingleAsync(
            x => x.Id == existingFolderPath.Id,
            CancellationToken
        );

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(existingFolderPath.Id);
        result.Value.DisplayName.ShouldBe(request.FolderPathDTO.DisplayName);
        result.Value.Directory.ShouldBe(request.FolderPathDTO.Directory);
        result.Value.FolderType.ShouldBe(request.FolderPathDTO.FolderType);
        result.Value.MediaType.ShouldBe(request.FolderPathDTO.MediaType);
        result.Value.IsDefault.ShouldBeFalse();
        updatedFolderPath.DisplayName.ShouldBe(request.FolderPathDTO.DisplayName);
        updatedFolderPath.DirectoryPath.ShouldBe(request.FolderPathDTO.Directory);
        updatedFolderPath.FolderType.ShouldBe(request.FolderPathDTO.FolderType);
        updatedFolderPath.MediaType.ShouldBe(request.FolderPathDTO.MediaType);
    }

    [Test]
    public async Task ShouldUpdateDefaultFolderPath_WhenFolderPathExists()
    {
        // Arrange
        await SetupDatabase(44202);

        var dbContext = IDbContext;
        var defaultFolderPathId = PlexMediaType.Movie.ToDefaultDestinationFolderId();
        var existingFolderPath = await dbContext.FolderPaths.SingleOrDefaultAsync(
            x => x.Id == defaultFolderPathId,
            CancellationToken
        );

        if (existingFolderPath is null)
        {
            existingFolderPath = new FolderPath
            {
                Id = defaultFolderPathId,
                DisplayName = "Default movie destination",
                DirectoryPath = "/tmp/default-movies-old",
                FolderType = FolderType.MovieFolder,
                MediaType = PlexMediaType.Movie,
            };

            dbContext.FolderPaths.Add(existingFolderPath);
            await dbContext.SaveChangesNewAsync(CancellationToken);
        }

        var request = new UpdateFolderPathEndpointRequest
        {
            FolderPathDTO = new FolderPathDTO
            {
                Id = defaultFolderPathId,
                DisplayName = "Updated default movie destination",
                Directory = "/tmp/default-movies-new",
                FolderType = FolderType.MovieFolder,
                MediaType = PlexMediaType.Movie,
                IsDefault = true,
                IsValid = true,
            },
        };

        // Act
        var endpointResult = await TestEndpointHandleAsync(request);
        var result = endpointResult.Response;
        var updatedFolderPath = await dbContext.FolderPaths.SingleAsync(
            x => x.Id == defaultFolderPathId,
            CancellationToken
        );

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(defaultFolderPathId);
        result.Value.DisplayName.ShouldBe(request.FolderPathDTO.DisplayName);
        result.Value.Directory.ShouldBe(request.FolderPathDTO.Directory);
        result.Value.FolderType.ShouldBe(request.FolderPathDTO.FolderType);
        result.Value.MediaType.ShouldBe(request.FolderPathDTO.MediaType);
        result.Value.IsDefault.ShouldBeTrue();
        updatedFolderPath.DisplayName.ShouldBe(request.FolderPathDTO.DisplayName);
        updatedFolderPath.DirectoryPath.ShouldBe(request.FolderPathDTO.Directory);
        updatedFolderPath.FolderType.ShouldBe(request.FolderPathDTO.FolderType);
        updatedFolderPath.MediaType.ShouldBe(request.FolderPathDTO.MediaType);
    }

    [Test]
    public async Task ShouldReturnEntityNotFound_WhenFolderPathDoesNotExist()
    {
        // Arrange
        await SetupDatabase(44203);

        var dbContext = IDbContext;
        var missingFolderPathId = 999;
        var request = new UpdateFolderPathEndpointRequest
        {
            FolderPathDTO = new FolderPathDTO
            {
                Id = missingFolderPathId,
                DisplayName = "Missing destination",
                Directory = "/tmp/missing",
                FolderType = FolderType.MovieFolder,
                MediaType = PlexMediaType.Movie,
                IsDefault = false,
                IsValid = true,
            },
        };
        var folderPathCountBefore = await dbContext.FolderPaths.CountAsync(CancellationToken);

        // Act
        var endpointResult = await TestEndpointHandleAsync(request);
        var result = endpointResult.Response;
        var folderPathCountAfter = await dbContext.FolderPaths.CountAsync(CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(x =>
            x.Message.Contains(nameof(FolderPath)) && x.Message.Contains(missingFolderPathId.ToString())
        );
        folderPathCountAfter.ShouldBe(folderPathCountBefore);
    }
}
