using PlexRipper.Settings.Models;
using Settings.Contracts;

namespace PlexRipper.Settings.Modules;

public class DebugSettingsModule : BaseSettingsModule<IDebugSettings>, IDebugSettingsModule
{
    #region Properties

    public bool DebugModeEnabled { get; set; }

    public bool MaskServerNames { get; set; }

    public bool MaskLibraryNames { get; set; }

    public override string Name => "DebugSettings";

    #endregion

    #region Methods

    #region Public

    public override IDebugSettings DefaultValues()
    {
        return new DebugSettings
        {
            DebugModeEnabled = false,
            MaskServerNames = false,
            MaskLibraryNames = false,
        };
    }

    public Result Update(IDebugSettingsModule sourceSettings)
    {
        DebugModeEnabled = sourceSettings.DebugModeEnabled;
        MaskServerNames = sourceSettings.MaskServerNames;
        MaskLibraryNames = sourceSettings.MaskLibraryNames;
        return Result.Ok();
    }

    public override IDebugSettings GetValues()
    {
        return new DebugSettings
        {
            DebugModeEnabled = DebugModeEnabled,
            MaskServerNames = MaskServerNames,
            MaskLibraryNames = MaskLibraryNames,
        };
    }

    #endregion

    #endregion
}
