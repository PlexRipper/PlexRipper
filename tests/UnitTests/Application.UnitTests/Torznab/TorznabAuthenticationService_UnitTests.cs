using Settings.Contracts;
using Shouldly;
using Xunit;

namespace PlexRipper.Application.UnitTests;

public class TorznabAuthenticationService_UnitTests : BaseUnitTest<TorznabAuthenticationService>
{
    public TorznabAuthenticationService_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void ValidateApiKey_WhenDisabled_ShouldReturnFalse()
    {
        // Arrange
        var userSettings = GetMock<IUserSettings>();
        userSettings.Setup(x => x.TorznabSettings.IsEnabled).Returns(false);

        var service = new TorznabAuthenticationService(userSettings.Object);

        // Act
        var result = service.ValidateApiKey("any-key");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ValidateApiKey_WhenEnabledAndCorrectKey_ShouldReturnTrue()
    {
        // Arrange
        var userSettings = GetMock<IUserSettings>();
        userSettings.Setup(x => x.TorznabSettings.IsEnabled).Returns(true);
        userSettings.Setup(x => x.TorznabSettings.ApiKey).Returns("correct-key");

        var service = new TorznabAuthenticationService(userSettings.Object);

        // Act
        var result = service.ValidateApiKey("correct-key");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateApiKey_WhenEnabledAndWrongKey_ShouldReturnFalse()
    {
        // Arrange
        var userSettings = GetMock<IUserSettings>();
        userSettings.Setup(x => x.TorznabSettings.IsEnabled).Returns(true);
        userSettings.Setup(x => x.TorznabSettings.ApiKey).Returns("correct-key");

        var service = new TorznabAuthenticationService(userSettings.Object);

        // Act
        var result = service.ValidateApiKey("wrong-key");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void GenerateNewApiKey_ShouldReturnValidGuid()
    {
        // Arrange
        var userSettings = GetMock<IUserSettings>();
        var service = new TorznabAuthenticationService(userSettings.Object);

        // Act
        var apiKey = service.GenerateNewApiKey();

        // Assert
        apiKey.ShouldNotBeNullOrEmpty();
        apiKey.Length.ShouldBe(32); // GUID without dashes
        Guid.TryParseExact(apiKey, "N", out _).ShouldBeTrue();
    }

    [Fact]
    public void IsEnabled_ShouldReturnCorrectValue()
    {
        // Arrange
        var userSettings = GetMock<IUserSettings>();
        userSettings.Setup(x => x.TorznabSettings.IsEnabled).Returns(true);

        var service = new TorznabAuthenticationService(userSettings.Object);

        // Act
        var isEnabled = service.IsEnabled;

        // Assert
        isEnabled.ShouldBeTrue();
    }
}