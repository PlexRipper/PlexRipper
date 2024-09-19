namespace PlexRipper.Domain;

public static class DateTimeExtensions
{
    public static DateTime FromUnixTime(long unixTime) => DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;

    public static DateTime? FromUnixTime(long? unixTime) =>
        unixTime != null ? DateTimeOffset.FromUnixTimeSeconds((long)unixTime).DateTime : null;
}
