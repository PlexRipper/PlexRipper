namespace PlexRipper.Application.Common
{
    public interface ISettingsModel
    {
        IAccountSettingsModel AccountSettings { get; set; }

        IAdvancedSettingsModel AdvancedSettings { get; set; }

        IUserInterfaceSettingsModel UserInterfaceSettings { get; set; }

        bool FirstTimeSetup { get; set; }
    }
}