namespace Reaparr.Application;

public static partial class QuartzExtensions
{
    public static JobDataMap ToJobDataMap<TPayload>(this TPayload payload)
    {
        var map = JsonSerializer.Deserialize<Dictionary<string, object>>(
            JsonSerializer.Serialize(payload, DefaultJsonSerializerOptions.ConfigStandard)
        );
        return new JobDataMap((IDictionary<string, object>)(map ?? []));
    }

    public static TPayload? GetPayload<TPayload>(this JobDataMap jobDataMap) =>
        JsonSerializer.Deserialize<TPayload>(
            JsonSerializer.Serialize(jobDataMap, DefaultJsonSerializerOptions.ConfigStandard)
        );
}
