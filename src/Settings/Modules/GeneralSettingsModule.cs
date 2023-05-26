using PlexRipper.Settings.Models;
using Settings.Contracts;

namespace PlexRipper.Settings.Modules;

public class GeneralSettingsModule : BaseSettingsModule<IGeneralSettings>, IGeneralSettingsModule
{
    public bool FirstTimeSetup { get; set; }

    public int ActiveAccountId { get; set; }

    public bool DebugMode { get; set; }

    public override string Name => "GeneralSettings";

    public override IGeneralSettings DefaultValues()
    {
        return new GeneralSettings
        {
            FirstTimeSetup = true,
            ActiveAccountId = 0,
            DebugMode = false,
        };
    }

    public Result Update(IGeneralSettingsModule sourceSettings)
    {
        FirstTimeSetup = sourceSettings.FirstTimeSetup;
        ActiveAccountId = sourceSettings.ActiveAccountId;
        DebugMode = sourceSettings.DebugMode;
        return Result.Ok();
    }

    public override IGeneralSettings GetValues()
    {
        return new GeneralSettings
        {
            FirstTimeSetup = FirstTimeSetup,
            ActiveAccountId = ActiveAccountId,
            DebugMode = DebugMode,
        };
    }
}