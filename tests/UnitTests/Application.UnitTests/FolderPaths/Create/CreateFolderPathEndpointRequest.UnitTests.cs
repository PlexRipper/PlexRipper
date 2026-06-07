namespace Reaparr.Application.UnitTests;

public class CreateFolderPathEndpointRequestUnitTests
    : BaseEndpointUnitTest<CreateFolderPathEndpoint, CreateFolderPathEndpointRequest, ResultDTO<FolderPathDTO>>
{
    [Test]
    public async Task ShouldRejectNullFolderPathDto_WhenCreatingFolderPath()
    {
        // Arrange
        var request = new CreateFolderPathEndpointRequest { FolderPathDto = null };

        // Act
        var result = await TestEndpointHandleAsync(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Response.ShouldBeNull();
        result.ValidationResult.ShouldNotBeNull();
        result.ValidationResult.Errors.ShouldContain(x =>
            x.PropertyName == nameof(CreateFolderPathEndpointRequest.FolderPathDto)
        );
    }

    [Test]
    public async Task ShouldAllowDownloadFolderWithNoneMediaType_WhenCreatingFolderPath()
    {
        // Arrange
        await SetupDatabase(45801);

        var request = new CreateFolderPathEndpointRequest
        {
            FolderPathDto = new FolderPathDTO
            {
                Id = 0,
                DisplayName = "Download path",
                Directory = @"G:\Reaparr\WIN\Downloads\",
                FolderType = FolderType.DownloadFolder,
                MediaType = PlexMediaType.None,
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

    [Test]
    public async Task ShouldRejectEmptyDirectory_WhenCreatingFolderPath()
    {
        // Arrange
        var request = new CreateFolderPathEndpointRequest
        {
            FolderPathDto = new FolderPathDTO
            {
                Id = 0,
                DisplayName = "Movies",
                Directory = string.Empty,
                FolderType = FolderType.MovieFolder,
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
        result.ValidationResult.Errors.ShouldContain(x => x.PropertyName == "FolderPathDto.Directory");
    }
}
