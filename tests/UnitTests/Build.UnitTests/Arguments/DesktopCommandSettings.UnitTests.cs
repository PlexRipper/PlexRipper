namespace Reaparr.Build.UnitTests;

public class DesktopCommandSettingsUnitTests : BaseUnitTest
{
    [Test]
    public void ShouldFailValidation_WhenRuntimeIdentifierIsMissing()
    {
        // Arrange
        var sut = new DesktopCommandSettings { Version = "1.2.3", InformationalVersion = "1.2.3-dev.1" };

        // Act
        var result = sut.Validate();

        // Assert
        result.Successful.ShouldBeFalse();
        result.Message.ShouldNotBeNull();
        result.Message.ShouldContain("runtime identifier is required");
    }

    [Test]
    public void ShouldFailValidation_WhenVersionIsMissing()
    {
        // Arrange
        var sut = new DesktopCommandSettings { RuntimeIdentifier = "linux-x64", InformationalVersion = "1.2.3-dev.1" };

        // Act
        var result = sut.Validate();

        // Assert
        result.Successful.ShouldBeFalse();
        result.Message.ShouldNotBeNull();
        result.Message.ShouldContain("build version is required");
    }

    [Test]
    public void ShouldFailValidation_WhenInformationalVersionIsMissing()
    {
        // Arrange
        var sut = new DesktopCommandSettings { RuntimeIdentifier = "linux-x64", Version = "1.2.3" };

        // Act
        var result = sut.Validate();

        // Assert
        result.Successful.ShouldBeFalse();
        result.Message.ShouldNotBeNull();
        result.Message.ShouldContain("informational version is required");
    }

    [Test]
    public void ShouldFailValidation_WhenRuntimeIdentifierIsUnsupported()
    {
        // Arrange
        var sut = new DesktopCommandSettings
        {
            RuntimeIdentifier = "freebsd-x64",
            Version = "1.2.3",
            InformationalVersion = "1.2.3-dev.1",
        };

        // Act
        var result = sut.Validate();

        // Assert
        result.Successful.ShouldBeFalse();
        result.Message.ShouldNotBeNull();
        result.Message.ShouldContain("Unsupported desktop RID 'freebsd-x64'");
    }

    [Test]
    public void ShouldFailValidation_WhenLinuxPackageLaunchModeIsUnsupported()
    {
        // Arrange
        var sut = new DesktopCommandSettings
        {
            RuntimeIdentifier = "linux-x64",
            Version = "1.2.3",
            InformationalVersion = "1.2.3-dev.1",
            LaunchMode = "invalid",
        };

        // Act
        var result = sut.Validate();

        // Assert
        result.Successful.ShouldBeFalse();
        result.Message.ShouldNotBeNull();
        result.Message.ShouldContain("Unsupported launch mode 'invalid'");
    }

    [Test]
    public void ShouldPassValidation_WhenNonLinuxRIDHasUnsupportedLaunchMode()
    {
        // Arrange
        var sut = new DesktopCommandSettings
        {
            RuntimeIdentifier = "win-x64",
            Version = "1.2.3",
            InformationalVersion = "1.2.3-dev.1",
            LaunchMode = "invalid",
        };

        // Act
        var result = sut.Validate();

        // Assert
        result.Successful.ShouldBeTrue();
        result.Message.ShouldBeNullOrWhiteSpace();
    }

    [Test]
    public void ShouldPassValidation_WhenRequiredSettingsAreValid()
    {
        // Arrange
        var sut = new DesktopCommandSettings
        {
            RuntimeIdentifier = "linux-x64",
            Version = "1.2.3",
            InformationalVersion = "1.2.3-dev.1",
        };

        // Act
        var result = sut.Validate();

        // Assert
        result.Successful.ShouldBeTrue();
    }
}
