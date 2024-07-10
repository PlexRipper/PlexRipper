namespace Settings.Contracts;

public class DateTimeSettingsDTO : IDateTimeSettings
{
    public required string ShortDateFormat { get; set; }

    public required string LongDateFormat { get; set; }

    public required string TimeFormat { get; set; }

    public required string TimeZone { get; set; }

    public required bool ShowRelativeDates { get; set; }
}
