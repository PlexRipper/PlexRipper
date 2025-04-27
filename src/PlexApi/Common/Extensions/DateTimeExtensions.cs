using NodaTime;

namespace PlexRipper.PlexApi;

public static class DateTimeExtensions
{
    public static DateTime FromUnixTime(long unixTime) => DateTimeOffset.FromUnixTimeMilliseconds(unixTime).DateTime;

    public static DateTime? FromUnixTime(long? unixTime) =>
        unixTime != null ? DateTimeOffset.FromUnixTimeSeconds((long)unixTime).DateTime : null;

    public static LocalDate ToLocalDate(this DateTime dateTime) => LocalDate.FromDateTime(dateTime);

    public static DateTime ToDateTime(this LocalDate localDate) => localDate.AtMidnight().ToDateTimeUnspecified();

    public static DateTime? ToDateTime(this LocalDate? localDate) => localDate?.AtMidnight().ToDateTimeUnspecified();

    public static long ToUnixLong(this DateTime dateTime) => new DateTimeOffset(dateTime).ToUnixTimeSeconds();
}
