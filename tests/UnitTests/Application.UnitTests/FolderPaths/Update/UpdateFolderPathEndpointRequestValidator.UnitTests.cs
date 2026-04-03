namespace Reaparr.Application.UnitTests;

public class UpdateFolderPathEndpointRequestValidatorUnitTests
{
    [Test]
    public void ShouldRejectNullFolderPathDto_WhenUpdatingFolderPath()
    {
        // Arrange
        var validator = new UpdateFolderPathEndpointRequestValidator();
        var request = new UpdateFolderPathEndpointRequest { FolderPathDto = null };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(UpdateFolderPathEndpointRequest.FolderPathDto));
    }

    [Test]
    public void ShouldRejectReservedFolderPathIds_WhenUpdatingFolderPath()
    {
        // Arrange
        var validator = new UpdateFolderPathEndpointRequestValidator();
        var request = new UpdateFolderPathEndpointRequest
        {
            FolderPathDto = new FolderPathDTO
            {
                Id = 5,
                DisplayName = "Reserved",
                Directory = "/tmp/reserved",
                FolderType = FolderType.DownloadFolder,
                MediaType = PlexMediaType.Movie,
                IsDefault = true,
                IsValid = true,
            },
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x =>
            x.ErrorMessage.Contains("reserved folder paths", StringComparison.OrdinalIgnoreCase)
        );
    }

    [Test]
    public void ShouldRejectNoneEnums_WhenUpdatingFolderPath()
    {
        // Arrange
        var validator = new UpdateFolderPathEndpointRequestValidator();
        var request = new UpdateFolderPathEndpointRequest
        {
            FolderPathDto = new FolderPathDTO
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
        result.Errors.ShouldContain(x => x.PropertyName == "FolderPathDto.FolderType");
        result.Errors.ShouldContain(x => x.PropertyName == "FolderPathDto.MediaType");
    }

    [Test]
    public void ShouldAllowId10_WhenUpdatingFolderPath()
    {
        // Arrange
        var validator = new UpdateFolderPathEndpointRequestValidator();
        var request = new UpdateFolderPathEndpointRequest
        {
            FolderPathDto = new FolderPathDTO
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
