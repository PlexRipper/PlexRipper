using System.Text.Json;
using Quartz;

namespace PlexRipper.Application;

public static class QuartzExtensions
{
    public static T? GetJsonValue<T>(this JobDataMap jobDataMap, string key)
    {
        var json = jobDataMap.GetString(key);
        return json == null ? default : JsonSerializer.Deserialize<T>(json);
    }
}