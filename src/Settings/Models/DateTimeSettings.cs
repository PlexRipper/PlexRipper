using Settings.Contracts;

namespace PlexRipper.Settings;

public class DateTimeSettings : IDateTimeSettings
{
    public string ShortDateFormat { get; set; }

    public string LongDateFormat { get; set; }

    public string TimeFormat { get; set; }

    public string TimeZone { get; set; }

    public bool ShowRelativeDates { get; set; }
}
