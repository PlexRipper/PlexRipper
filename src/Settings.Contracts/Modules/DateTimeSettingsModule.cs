namespace Reaparr.Settings.Contracts;

public record DateTimeSettingsModule
    : BaseSettingsModule<DateTimeSettingsModule>,
        IBaseSettingsModule<DateTimeSettingsModule>,
        IDateTimeSettings
{
    public static DateTimeSettingsModule Create() =>
        new()
        {
            ShortDateFormat = "dd/MM/yyyy",
            LongDateFormat = "EEEE, dd MMMM yyyy",
            TimeFormat = "HH:mm:ss",
            TimeZone = "UTC",
            ShowRelativeDates = true,
        };

    public required string ShortDateFormat
    {
        get;
        set => SetProperty(ref field, value);
    } = "dd/MM/yyyy";

    public required string LongDateFormat
    {
        get;
        set => SetProperty(ref field, value);
    } = "EEEE, dd MMMM yyyy";

    public required string TimeFormat
    {
        get;
        set => SetProperty(ref field, value);
    } = "HH:mm:ss";

    public required string TimeZone
    {
        get;
        set => SetProperty(ref field, value);
    } = "UTC";

    public required bool ShowRelativeDates
    {
        get;
        set => SetProperty(ref field, value);
    } = true;
}
