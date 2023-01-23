using PlexRipper.Application;
using PlexRipper.Settings.Models;
using Settings.Contracts;

namespace PlexRipper.Settings.Modules;

public class GeneralSettingsModule : BaseSettingsModule<IGeneralSettings>, IGeneralSettingsModule
{
    public bool FirstTimeSetup { get; set; }

    public int ActiveAccountId { get; set; }

    public override string Name => "GeneralSettings";

    public override IGeneralSettings DefaultValues()
    {
        return new GeneralSettings
        {
            FirstTimeSetup = true,
            ActiveAccountId = 0,
        };
    }

    public Result Update(IGeneralSettingsModule sourceSettings)
    {
        FirstTimeSetup = sourceSettings.FirstTimeSetup;
        return Result.Ok();
    }

    public override IGeneralSettings GetValues()
    {
        return new GeneralSettings
        {
            FirstTimeSetup = FirstTimeSetup,
        };
    }
}