namespace Reaparr.Application.UnitTests;

public class UpdateFolderPathEndpointRequestUnitTests
    : BaseEndpointUnitTest<UpdateFolderPathEndpoint, UpdateFolderPathEndpointRequest, ResultDTO<FolderPathDTO>>
{
    [Test]
    public async Task ShouldRejectNullFolderPathDto_WhenUpdatingFolderPath()
    {
        // Arrange
        var request = new UpdateFolderPathEndpointRequest { FolderPathDTO = null! };

        // Act
        var result = await TestEndpointHandleAsync(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Response.ShouldBeNull();
        result.ValidationResult.ShouldNotBeNull();
        result.ValidationResult.Errors.ShouldContain(x =>
            x.PropertyName == nameof(UpdateFolderPathEndpointRequest.FolderPathDTO)
        );
    }

    [Test]
    public async Task ShouldAllowDefaultFolderPath_WhenUpdatingFolderPath()
    {
        // Arrange
        await SetupDatabase(45811);

        var request = new UpdateFolderPathEndpointRequest
        {
            FolderPathDTO = new FolderPathDTO
            {
                Id = PlexMediaType.Movie.ToDefaultDestinationFolderId(),
                DisplayName = "Default movie destination",
                Directory = "/tmp/default-movies",
                FolderType = FolderType.MovieFolder,
                MediaType = PlexMediaType.Movie,
                IsDefault = true,
                IsValid = true,
            },
        };

        // Act
        var result = await TestEndpointHandleAsync(request);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Response.ShouldNotBeNull();
        result.Response.IsSuccess.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldAllowDownloadFolderWithNoneMediaType_WhenUpdatingFolderPath()
    {
        // Arrange
        await SetupDatabase(45812);

        var request = new UpdateFolderPathEndpointRequest
        {
            FolderPathDTO = new FolderPathDTO
            {
                Id = FolderTypeDefaults.DefaultDownloadFolderId,
                DisplayName = "Download path",
                Directory = @"G:\Reaparr\WIN\Downloads\",
                FolderType = FolderType.DownloadFolder,
                MediaType = PlexMediaType.None,
                IsDefault = true,
                IsValid = true,
            },
        };

        // Act
        var result = await TestEndpointHandleAsync(request);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Response.ShouldNotBeNull();
        result.Response.IsSuccess.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldRejectNoneMediaTypeForNonDownloadFolder_WhenUpdatingFolderPath()
    {
        // Arrange
        var request = new UpdateFolderPathEndpointRequest
        {
            FolderPathDTO = new FolderPathDTO
            {
                Id = 11,
                DisplayName = "Movie destination",
                Directory = @"G:\Reaparr\WIN\Movies\",
                FolderType = FolderType.MovieFolder,
                MediaType = PlexMediaType.None,
                IsDefault = false,
                IsValid = true,
            },
        };

        // Act
        var result = await TestEndpointHandleAsync(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Response.ShouldBeNull();
        result.ValidationResult.ShouldNotBeNull();
        result.ValidationResult.Errors.ShouldContain(x => x.PropertyName == "FolderPathDTO.MediaType");
    }

    [Test]
    public async Task ShouldRejectNoneFolderType_WhenUpdatingFolderPath()
    {
        // Arrange
        var request = new UpdateFolderPathEndpointRequest
        {
            FolderPathDTO = new FolderPathDTO
            {
                Id = 11,
                DisplayName = "Invalid enum values",
                Directory = "/tmp/invalid",
                FolderType = FolderType.None,
                MediaType = PlexMediaType.Movie,
                IsDefault = false,
                IsValid = true,
            },
        };

        // Act
        var result = await TestEndpointHandleAsync(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Response.ShouldBeNull();
        result.ValidationResult.ShouldNotBeNull();
        result.ValidationResult.Errors.ShouldContain(x => x.PropertyName == "FolderPathDTO.FolderType");
    }

    [Test]
    public async Task ShouldAllowId10_WhenUpdatingFolderPath()
    {
        // Arrange
        await SetupDatabase(45813);

        var dbContext = IDbContext;
        var existingFolderPath = await dbContext.FolderPaths.SingleAsync(x => x.Id == 10, CancellationToken);

        var request = new UpdateFolderPathEndpointRequest
        {
            FolderPathDTO = new FolderPathDTO
            {
                Id = existingFolderPath.Id,
                DisplayName = "Allowed boundary",
                Directory = "/tmp/allowed",
                FolderType = FolderType.MovieFolder,
                MediaType = PlexMediaType.Movie,
                IsDefault = false,
                IsValid = true,
            },
        };

        // Act
        var result = await TestEndpointHandleAsync(request);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Response.ShouldNotBeNull();
        result.Response.IsSuccess.ShouldBeTrue();
    }
}
