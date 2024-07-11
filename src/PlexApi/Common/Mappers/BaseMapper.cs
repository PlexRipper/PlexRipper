namespace PlexRipper.PlexApi;

public static class BaseMapper
{
    public static DateTime? ToDateTime(this string source)
    {
        if (DateTime.TryParse(source, out var dateTimeResult))
            return dateTimeResult;

        return null;
    }
}
