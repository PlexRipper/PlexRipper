using Reaparr.Settings.Contracts;
using Serilog;

namespace Reaparr.BaseTests;

public class MockConfigManager : IConfigManager
{
    private readonly Serilog.ILogger _log;
    private readonly IUserSettings _userSettings;

    public MockConfigManager(ILogger log, IUserSettings userSettings)
    {
        _log = log;
        _userSettings = userSettings;
    }

    public Result Setup()
    {
        _log.Information("Setting up default user config settings in integration mode");
        _userSettings.Reset();
        return Result.Ok();
    }

    public Result SaveConfig() => Result.Ok();

    public Result ResetConfig()
    {
        _userSettings.Reset();
        return Result.Ok();
    }

    public Result LoadConfig() => Result.Ok();

    public bool ConfigFileExists() => false;
}
