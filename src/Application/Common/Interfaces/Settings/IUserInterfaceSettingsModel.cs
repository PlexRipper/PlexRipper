namespace PlexRipper.Application.Common
{
    public interface IUserInterfaceSettingsModel
    {
        IConfirmationSettingsModel ConfirmationSettings { get; set; }

        IDisplaySettingsModel DisplaySettings { get; set; }

        IDateTimeModel DateTimeSettings { get; set; }
    }
}