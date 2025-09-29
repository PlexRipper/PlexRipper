using Reaparr.Settings.Contracts;

namespace Reaparr.Settings.UnitTests;

public class HeaderAuthenticationSettingsUnitTests
{
    [Fact]
    public void ShouldCreateWithDefaultValues()
    {
        // Act
        var settings = HeaderAuthenticationSettings.Create();

        // Assert
        settings.Enabled.ShouldBeFalse();
        settings.HeaderName.ShouldBe("X-Auth-User");
        settings.MappingType.ShouldBe(HeaderMappingType.Username);
        settings.CustomMappingExpression.ShouldBeNull();
        settings.AutoCreateUsers.ShouldBeFalse();
        settings.DefaultRole.ShouldBe("Admin");
        settings.TrustedProxies.ShouldBeEmpty();
        settings.EnableLogging.ShouldBeTrue();
        settings.MaxHeaderLength.ShouldBe(256);
        settings.RequireHttps.ShouldBeTrue();
    }

    [Fact]
    public void ShouldBeValid_WhenDisabled()
    {
        // Arrange
        var settings = HeaderAuthenticationSettings.Create();
        settings.Enabled = false;

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void ShouldBeValid_WhenEnabledWithValidConfiguration()
    {
        // Arrange
        var settings = HeaderAuthenticationSettings.Create();
        settings.Enabled = true;
        settings.HeaderName = "X-Auth-User";
        settings.MaxHeaderLength = 100;
        settings.DefaultRole = "User";
        settings.TrustedProxies = new List<string> { "192.168.1.0/24", "10.0.0.1" };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void ShouldBeInvalid_WhenEnabledWithEmptyHeaderName()
    {
        // Arrange
        var settings = HeaderAuthenticationSettings.Create();
        settings.Enabled = true;
        settings.HeaderName = "";

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void ShouldBeInvalid_WhenEnabledWithZeroMaxHeaderLength()
    {
        // Arrange
        var settings = HeaderAuthenticationSettings.Create();
        settings.Enabled = true;
        settings.MaxHeaderLength = 0;

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void ShouldBeInvalid_WhenEnabledWithEmptyDefaultRole()
    {
        // Arrange
        var settings = HeaderAuthenticationSettings.Create();
        settings.Enabled = true;
        settings.DefaultRole = "";

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void ShouldBeInvalid_WhenTrustedProxyContainsInvalidIp()
    {
        // Arrange
        var settings = HeaderAuthenticationSettings.Create();
        settings.Enabled = true;
        settings.TrustedProxies = new List<string> { "invalid-ip" };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void ShouldBeInvalid_WhenTrustedProxyContainsInvalidCidr()
    {
        // Arrange
        var settings = HeaderAuthenticationSettings.Create();
        settings.Enabled = true;
        settings.TrustedProxies = new List<string> { "192.168.1.0/33" };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void ShouldBeValid_WithValidCidrRanges()
    {
        // Arrange
        var settings = HeaderAuthenticationSettings.Create();
        settings.Enabled = true;
        settings.TrustedProxies = new List<string> 
        { 
            "192.168.1.0/24", 
            "10.0.0.0/8", 
            "172.16.0.0/12",
            "127.0.0.1",
            "::1"
        };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("192.168.1.1", true)]
    [InlineData("10.0.0.1", true)]
    [InlineData("invalid-ip", false)]
    [InlineData("", false)]
    [InlineData("192.168.1.0/24", true)]
    [InlineData("192.168.1.0/33", false)]
    [InlineData("192.168.1.0/", false)]
    [InlineData("192.168.1.0/24/extra", false)]
    public void ShouldValidateIpOrCidrCorrectly(string input, bool expectedValid)
    {
        // Arrange
        var settings = HeaderAuthenticationSettings.Create();
        settings.Enabled = true;
        settings.TrustedProxies = new List<string> { input };

        // Act
        var isValid = settings.IsValid();

        // Assert
        isValid.ShouldBe(expectedValid);
    }

    [Fact]
    public void ShouldSupportAllMappingTypes()
    {
        // Arrange & Act
        var usernameMapping = HeaderAuthenticationSettings.Create();
        usernameMapping.MappingType = HeaderMappingType.Username;

        var emailMapping = HeaderAuthenticationSettings.Create();
        emailMapping.MappingType = HeaderMappingType.Email;

        var customMapping = HeaderAuthenticationSettings.Create();
        customMapping.MappingType = HeaderMappingType.Custom;

        // Assert
        usernameMapping.MappingType.ShouldBe(HeaderMappingType.Username);
        emailMapping.MappingType.ShouldBe(HeaderMappingType.Email);
        customMapping.MappingType.ShouldBe(HeaderMappingType.Custom);
    }

    [Fact]
    public void ShouldAllowCustomMappingExpression()
    {
        // Arrange
        var settings = HeaderAuthenticationSettings.Create();
        var customExpression = "headerValue.ToLower().Replace('-', '_')";

        // Act
        settings.CustomMappingExpression = customExpression;

        // Assert
        settings.CustomMappingExpression.ShouldBe(customExpression);
    }

    [Fact]
    public void ShouldSupportAutoCreateUsersConfiguration()
    {
        // Arrange
        var settings = HeaderAuthenticationSettings.Create();

        // Act
        settings.AutoCreateUsers = true;
        settings.DefaultRole = "User";

        // Assert
        settings.AutoCreateUsers.ShouldBeTrue();
        settings.DefaultRole.ShouldBe("User");
    }

    [Fact]
    public void ShouldSupportLoggingConfiguration()
    {
        // Arrange
        var settings = HeaderAuthenticationSettings.Create();

        // Act
        settings.EnableLogging = false;

        // Assert
        settings.EnableLogging.ShouldBeFalse();
    }

    [Fact]
    public void ShouldSupportHttpsRequirementConfiguration()
    {
        // Arrange
        var settings = HeaderAuthenticationSettings.Create();

        // Act
        settings.RequireHttps = false;

        // Assert
        settings.RequireHttps.ShouldBeFalse();
    }

    [Fact]
    public void ShouldSupportMaxHeaderLengthConfiguration()
    {
        // Arrange
        var settings = HeaderAuthenticationSettings.Create();

        // Act
        settings.MaxHeaderLength = 512;

        // Assert
        settings.MaxHeaderLength.ShouldBe(512);
    }
}
