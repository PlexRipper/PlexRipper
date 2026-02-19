namespace Reaparr.Domain;

public static class TimeSpanExtensions
{
    public static string ToFormattedString(this TimeSpan timeSpan) => timeSpan.ToString(@"hh\:mm\:ss\.fff");
}
