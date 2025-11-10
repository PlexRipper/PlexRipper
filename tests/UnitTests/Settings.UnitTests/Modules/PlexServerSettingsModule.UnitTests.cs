using Reaparr.Settings.Contracts;

namespace Reaparr.Settings.UnitTests;

public class PlexServerSettingsModuleUnitTests : BaseUnitTest<PlexServerSettingsModule>
{
    public PlexServerSettingsModuleUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldRenameAServerByItsMachineIdentifier_WhenItDoesNotExist()
    {
        // Arrange
        var sut = PlexServerSettingsModule.Create();
        var machineIdentifier = "test";

        // Act
        sut.SetServerName(machineIdentifier, "test-name");

        // Assert
        sut.GetServerNameAlias(machineIdentifier).ShouldBe("test-name");
        sut.Data.Count.ShouldBe(1);
    }

    [Fact]
    public void ShouldRenameAServerByItsMachineIdentifier_WhenItAlreadyExists()
    {
        // Arrange
        var sut = PlexServerSettingsModule.Create();
        var machineIdentifier = "test";
        sut.SetServerName(machineIdentifier, "test-name");

        // Act
        sut.SetServerName(machineIdentifier, "test-name-2");

        // Assert
        sut.GetServerNameAlias(machineIdentifier).ShouldBe("test-name-2");
        sut.Data.Count.ShouldBe(1);
    }

    [Fact]
    public void GetDownloadSpeedLimit_ShouldReturnDefaultWhenNotSet()
    {
        // Arrange
        var sut = PlexServerSettingsModule.Create();

        // Act
        var speedLimit = sut.GetDownloadSpeedLimit("machine1");

        // Assert
        speedLimit.ShouldBe(0);
    }

    [Fact]
    public void SetServerHiddenState_ShouldUpdateValue()
    {
        // Arrange
        var sut = PlexServerSettingsModule.Create();

        sut.SetServerHiddenState("machine1", true);

        // Act
        var result = sut.Data.FirstOrDefault(x => x.MachineIdentifier == "machine1");

        // Assert
        result.ShouldNotBeNull();
        result.Hidden.ShouldBeTrue();
    }

    [Fact]
    public void ShouldEmitValuesWhenChanged_WhenSubscribedToTheObservable()
    {
        // Arrange
        var sut = PlexServerSettingsModule.Create();

        var emittedValues = new List<int>();
        var subscription = sut.GetDownloadSpeedLimitObservable("machine1").Subscribe(emittedValues.Add);

        // Act
        sut.SetDownloadSpeedLimit("machine1", 200);
        sut.SetDownloadSpeedLimit("machine2", 500);
        sut.SetDownloadSpeedLimit("machine1", 300);

        // Assert
        emittedValues[0].ShouldBe(0);
        emittedValues[1].ShouldBe(200);
        emittedValues[2].ShouldBe(300);

        subscription.Dispose();
    }
}
