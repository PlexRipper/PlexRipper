namespace Settings.Contracts;

public interface IGeneralSettings
{
    bool FirstTimeSetup { get; set; }

    int ActiveAccountId { get; set; }

    bool DebugMode { get; set; }
    bool DisableAnimatedBackground { get; set; }
}