namespace Settings.Contracts;

public class GeneralSettingsDTO : IGeneralSettings
{
    public bool FirstTimeSetup { get; set; }

    public int ActiveAccountId { get; set; }
    public bool DebugMode { get; set; }
}