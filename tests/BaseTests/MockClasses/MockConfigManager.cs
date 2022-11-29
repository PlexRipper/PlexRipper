using PlexRipper.Application;

namespace PlexRipper.BaseTests;

public class MockConfigManager : IConfigManager
{
    private readonly IUserSettings _userSettings;

    public MockConfigManager(IUserSettings userSettings)
    {
        _userSettings = userSettings;
    }

    public Result Setup()
    {
        Log.Information("Setting up default user config settings in integration mode");
        _userSettings.Reset();
        return Result.Ok();
    }

    public Result SaveConfig()
    {
        return Result.Ok();
    }

    public Result ResetConfig()
    {
        _userSettings.Reset();
        return Result.Ok();
    }

    public Result LoadConfig()
    {
        return Result.Ok();
    }

    public bool ConfigFileExists()
    {
        return false;
    }
}