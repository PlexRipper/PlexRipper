using System.Text.Json;
using Quartz;

namespace PlexRipper.Application;

public static class QuartzExtensions
{
    public static T GetJsonValue<T>(this JobDataMap jobDataMap, string key) => JsonSerializer.Deserialize<T>(jobDataMap.GetString(key));
}