namespace Reaparr.Application.UnitTests;

public class SearchPlexMediaRequestValidatorUnitTests
{
    [Test]
    public void ShouldRejectNullQuery_WhenSearchingPlexMedia()
    {
        // Arrange
        var validator = new SearchPlexMediaRequestValidator();
        var request = new SearchPlexMediaRequest { Query = null! };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == "Query");
    }

    [Test]
    public void ShouldRejectEmptyQuery_WhenSearchingPlexMedia()
    {
        // Arrange
        var validator = new SearchPlexMediaRequestValidator();
        var request = new SearchPlexMediaRequest { Query = string.Empty };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == "Query");
    }
}
