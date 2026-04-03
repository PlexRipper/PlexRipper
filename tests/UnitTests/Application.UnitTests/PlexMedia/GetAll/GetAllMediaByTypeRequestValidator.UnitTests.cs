namespace Reaparr.Application.UnitTests;

public class GetAllMediaByTypeRequestValidatorUnitTests
{
    [Test]
    public void ShouldRejectNonZeroPage_WhenSizeIsZero()
    {
        // Arrange
        var validator = new GetAllMediaByTypeRequestValidator();
        var request = new GetAllMediaByTypeRequest(
            mediaType: PlexMediaType.Movie,
            page: 1,
            size: 0,
            filterOfflineMedia: false,
            filterOwnedMedia: false
        );

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x =>
            x.ErrorMessage.Contains("Page must be 0 when size is 0.", StringComparison.Ordinal)
        );
    }

    [Test]
    public void ShouldAllowZeroPage_WhenSizeIsZero()
    {
        // Arrange
        var validator = new GetAllMediaByTypeRequestValidator();
        var request = new GetAllMediaByTypeRequest(
            mediaType: PlexMediaType.Movie,
            page: 0,
            size: 0,
            filterOfflineMedia: false,
            filterOwnedMedia: false
        );

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }
}
