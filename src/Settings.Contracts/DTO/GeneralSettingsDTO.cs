namespace Settings.Contracts;

public class GeneralSettingsDTO : IGeneralSettings
{
    public required bool FirstTimeSetup { get; set; }

    public required int ActiveAccountId { get; set; }

    // TODO Remove this as this is now part of the DebugSettingsDTO
    public required bool DebugMode { get; set; }

    public required bool DisableAnimatedBackground { get; set; }
}
