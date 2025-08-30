namespace Reaparr.Settings.Contracts;

public record DateTimeSettingsModule : BaseSettingsModule<DateTimeSettingsModule>, IDateTimeSettings
{
    private string _shortDateFormat = "dd/MM/yyyy";
    private string _longDateFormat = "EEEE, dd MMMM yyyy";
    private string _timeFormat = "HH:mm:ss";
    private string _timeZone = "UTC";
    private bool _showRelativeDates = true;

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
        get => _shortDateFormat;
        set => SetProperty(ref _shortDateFormat, value);
    }

    public required string LongDateFormat
    {
        get => _longDateFormat;
        set => SetProperty(ref _longDateFormat, value);
    }

    public required string TimeFormat
    {
        get => _timeFormat;
        set => SetProperty(ref _timeFormat, value);
    }

    public required string TimeZone
    {
        get => _timeZone;
        set => SetProperty(ref _timeZone, value);
    }

    public required bool ShowRelativeDates
    {
        get => _showRelativeDates;
        set => SetProperty(ref _showRelativeDates, value);
    }
}
