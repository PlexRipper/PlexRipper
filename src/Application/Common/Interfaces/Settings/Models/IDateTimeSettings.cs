namespace PlexRipper.Application
{
    public interface IDateTimeSettings
    {
        string ShortDateFormat { get; set; }

        string LongDateFormat { get; set; }

        string TimeFormat { get; set; }

        string TimeZone { get; set; }

        bool ShowRelativeDates { get; set; }
    }
}