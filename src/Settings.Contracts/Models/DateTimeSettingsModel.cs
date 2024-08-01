namespace Settings.Contracts;

public record DateTimeSettingsModel : BaseSettingsModel<DateTimeSettingsModel>, IDateTimeSettings
{
    private string _shortDateFormat = "dd/MM/yyyy";
    private string _longDateFormat = "EEEE, dd MMMM yyyy";
    private string _timeFormat = "HH:mm:ss";
    private string _timeZone = "UTC";
    private bool _showRelativeDates = true;

    public string ShortDateFormat
    {
        get => _shortDateFormat;
        set => SetProperty(ref _shortDateFormat, value);
    }

    public string LongDateFormat
    {
        get => _longDateFormat;
        set => SetProperty(ref _longDateFormat, value);
    }

    public string TimeFormat
    {
        get => _timeFormat;
        set => SetProperty(ref _timeFormat, value);
    }

    public string TimeZone
    {
        get => _timeZone;
        set => SetProperty(ref _timeZone, value);
    }

    public bool ShowRelativeDates
    {
        get => _showRelativeDates;
        set => SetProperty(ref _showRelativeDates, value);
    }
}
