using Settings.Contracts;

namespace Settings.UnitTests.Modules;

public class PlexServerSettingsModule_UnitTests : BaseUnitTest<PlexServerSettingsModule>
{
    public PlexServerSettingsModule_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldRenameAServerByItsMachineIdentifier_WhenItDoesNotExist()
    {
        // Arrange
        var sut = new PlexServerSettingsModule();
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
        var sut = new PlexServerSettingsModule();
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
        var sut = new PlexServerSettingsModule();

        // Act
        var speedLimit = sut.GetDownloadSpeedLimit("machine1");

        // Assert
        speedLimit.ShouldBe(0);
    }

    [Fact]
    public void SetServerHiddenState_ShouldUpdateValue()
    {
        // Arrange
        var sut = new PlexServerSettingsModule();

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
        var sut = new PlexServerSettingsModule();

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
