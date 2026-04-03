namespace Reaparr.Application.UnitTests;

public class SetServerAliasRequestValidatorUnitTests
{
    [Test]
    public void ShouldRejectEmptyAlias_WhenSettingServerAlias()
    {
        // Arrange
        var validator = new SetServerAliasRequestValidator();
        var request = new SetServerAliasRequest { PlexServerId = 1, ServerAlias = string.Empty };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == "ServerAlias");
    }

    [Test]
    public void ShouldRejectWhitespaceAlias_WhenSettingServerAlias()
    {
        // Arrange
        var validator = new SetServerAliasRequestValidator();
        var request = new SetServerAliasRequest { PlexServerId = 1, ServerAlias = "   " };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == "ServerAlias");
    }
}
