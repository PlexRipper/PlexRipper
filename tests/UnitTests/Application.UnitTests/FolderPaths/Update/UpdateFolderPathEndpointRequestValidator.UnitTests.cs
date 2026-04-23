namespace Reaparr.Application.UnitTests;

public class UpdateFolderPathEndpointRequestValidatorUnitTests
{
    [Test]
    public void ShouldRejectNullFolderPathDto_WhenUpdatingFolderPath()
    {
        // Arrange
        var validator = new UpdateFolderPathEndpointRequestValidator();
        var request = new UpdateFolderPathEndpointRequest { FolderPathDTO = null! };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(UpdateFolderPathEndpointRequest.FolderPathDTO));
    }

    [Test]
    public void ShouldAllowDefaultFolderPath_WhenUpdatingFolderPath()
    {
        // Arrange
        var validator = new UpdateFolderPathEndpointRequestValidator();
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
        var result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldRejectNoneEnums_WhenUpdatingFolderPath()
    {
        // Arrange
        var validator = new UpdateFolderPathEndpointRequestValidator();
        var request = new UpdateFolderPathEndpointRequest
        {
            FolderPathDTO = new FolderPathDTO
            {
                Id = 11,
                DisplayName = "Invalid enum values",
                Directory = "/tmp/invalid",
                FolderType = FolderType.None,
                MediaType = PlexMediaType.None,
                IsDefault = false,
                IsValid = true,
            },
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == "FolderPathDTO.FolderType");
        result.Errors.ShouldContain(x => x.PropertyName == "FolderPathDTO.MediaType");
    }

    [Test]
    public void ShouldAllowId10_WhenUpdatingFolderPath()
    {
        // Arrange
        var validator = new UpdateFolderPathEndpointRequestValidator();
        var request = new UpdateFolderPathEndpointRequest
        {
            FolderPathDTO = new FolderPathDTO
            {
                Id = 10,
                DisplayName = "Allowed boundary",
                Directory = "/tmp/allowed",
                FolderType = FolderType.MovieFolder,
                MediaType = PlexMediaType.Movie,
                IsDefault = false,
                IsValid = true,
            },
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }
}
