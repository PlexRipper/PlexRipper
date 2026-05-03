namespace Reaparr.Build.UnitTests;

internal class DesktopPackageCommandUnitTests
{
    [Test]
    public async Task ShouldFailValidation_WhenSettingsIsNull()
    {
        // Arrange
        var validator = new DesktopPackageBuildCommandValidator();

        // Act
        var result = await validator.ValidateAsync(new DesktopPackageBuildCommand(null!));

        // Assert
        result.IsValid.ShouldBeFalse();
    }
}
