namespace Reaparr.Application.UnitTests;

public class UpdateUserSettingsEndpointUnitTests
{
    [Test]
    public void UpdateUserSettingsEndpointRequestValidator_ShouldRejectNullSettingsModelDto()
    {
        // Arrange
        var validator = new UpdateUserSettingsEndpointRequestValidator();
        var request = new UpdateUserSettingsEndpointRequest { SettingsModelDto = null };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(UpdateUserSettingsEndpointRequest.SettingsModelDto));
    }
}
