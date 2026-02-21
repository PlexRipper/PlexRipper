namespace Reaparr.Domain;

public static class TimeSpanExtensions
{
    public static string ToFormattedString(this TimeSpan timeSpan)
    {
        var hours = (int)timeSpan.TotalHours;
        return $"{hours:D2}:{timeSpan:mm\\:ss\\.fff}";
    }
}
