using Settings.Contracts;

namespace PlexRipper.Settings;

public record DateTimeSettings : IDateTimeSettings
{
    public string ShortDateFormat { get; set; } = "dd/MM/yyyy";

    public string LongDateFormat { get; set; } = "EEEE, dd MMMM yyyy";

    public string TimeFormat { get; set; } = "HH:mm:ss";

    public string TimeZone { get; set; } = "UTC";

    public bool ShowRelativeDates { get; set; } = true;
}
