using Logging.Interface;
using Settings.Contracts;

namespace PlexRipper.BaseTests;

public class MockConfigManager : IConfigManager
{
    private readonly ILog _log;
    private readonly IUserSettings _userSettings;

    public MockConfigManager(ILog log, IUserSettings userSettings)
    {
        _log = log;
        _userSettings = userSettings;
    }

    public Result Setup()
    {
        _log.InformationLine("Setting up default user config settings in integration mode");
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
