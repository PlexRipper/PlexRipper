namespace Reaparr.Application.UnitTests;

public class UpdatePlexAccountByIdEndpointUnitTests
{
    [Test]
    public void UpdatePlexAccountByIdEndpointRequestValidator_ShouldRejectNullPlexAccountDto()
    {
        // Arrange
        var validator = new UpdatePlexAccountByIdEndpointRequestValidator();
        var request = new UpdatePlexAccountByIdEndpointRequest { PlexAccountDTO = null };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(UpdatePlexAccountByIdEndpointRequest.PlexAccountDTO));
    }
}
