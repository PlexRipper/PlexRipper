namespace Reaparr.Build.UnitTests;

internal class DesktopRunCommandUnitTests
{
    [Test]
    public async Task ShouldFailValidation_WhenSettingsIsNull()
    {
        // Arrange
        var validator = new DesktopRunBuildCommandValidator();

        // Act
        var result = await validator.ValidateAsync(new DesktopRunBuildCommand(null!));

        // Assert
        result.IsValid.ShouldBeFalse();
    }
}
