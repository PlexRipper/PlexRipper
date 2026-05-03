namespace Reaparr.Build.UnitTests;

internal class DesktopPublishCommandUnitTests
{
    [Test]
    public async Task ShouldFailValidation_WhenSettingsIsNull()
    {
        // Arrange
        var validator = new DesktopPublishBuildCommandValidator();

        // Act
        var result = await validator.ValidateAsync(new DesktopPublishBuildCommand(null!));

        // Assert
        result.IsValid.ShouldBeFalse();
    }
}
