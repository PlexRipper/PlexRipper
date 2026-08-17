namespace Reaparr.Application;

public static partial class QuartzExtensions
{
    /// <summary>
    /// Convert a typed payload to <see cref="JobDataMap"/>
    /// </summary>
    /// <param name="payload">The typed payload to convert</param>
    /// <typeparam name="TPayload">The type of the payload</typeparam>
    /// <returns>The converted JobDataMap</returns>
    /// <exception cref="InvalidOperationException">Thrown when the payload is not a JSON object</exception>
    public static JobDataMap ToJobDataMap<TPayload>(this TPayload payload)
    {
        using var document = JsonDocument.Parse(
            JsonSerializer.Serialize(payload, DefaultJsonSerializerOptions.ConfigStandard)
        );
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException($"Quartz payload {typeof(TPayload).Name} must be a JSON object");

        var map = new JobDataMap();
        foreach (var property in document.RootElement.EnumerateObject())
            map[property.Name] = property.Value.GetRawText();

        return map;
    }

    /// <summary>
    /// Get the typed payload from <see cref="JobDataMap"/>
    /// </summary>
    /// <param name="jobDataMap">The JobDataMap to convert</param>
    /// <typeparam name="TPayload">The type of the payload</typeparam>
    /// <returns>The converted payload</returns>
    public static TPayload? GetPayload<TPayload>(this JobDataMap jobDataMap)
    {
        var payloadJson = jobDataMap
            .Where(x => !x.Key.Contains("QRTZ_"))
            .ToDictionary(
                x => x.Key,
                x =>
                {
                    var value = x.Value.ToString();

                    if (bool.TryParse(value, out var boolValue))
                        return (object)boolValue;

                    if (int.TryParse(value, out var intValue))
                        return intValue;

                    return value;
                }
            );

        return JsonSerializer.Deserialize<TPayload>(
            JsonSerializer.Serialize(payloadJson, DefaultJsonSerializerOptions.ConfigStandard),
            DefaultJsonSerializerOptions.ConfigStandard
        );
    }
}
