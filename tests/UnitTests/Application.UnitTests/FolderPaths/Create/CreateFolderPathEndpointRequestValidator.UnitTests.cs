namespace Reaparr.Application.UnitTests;

public class CreateFolderPathEndpointRequestValidatorUnitTests
{
    [Test]
    public void ShouldRejectNullFolderPathDto_WhenCreatingFolderPath()
    {
        // Arrange
        var validator = new CreateFolderPathEndpointRequestValidator();
        var request = new CreateFolderPathEndpointRequest { FolderPathDto = null };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(CreateFolderPathEndpointRequest.FolderPathDto));
    }

    [Test]
    public void ShouldRejectEmptyDirectory_WhenCreatingFolderPath()
    {
        // Arrange
        var validator = new CreateFolderPathEndpointRequestValidator();
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
        var result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == "FolderPathDto.Directory");
    }
}
