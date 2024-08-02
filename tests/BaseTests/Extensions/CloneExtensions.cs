using System.Text.Json;

namespace PlexRipper.BaseTests;

public static class CloneExtensions
{
    public static T CloneJson<T>(this T source)
    {
        // Don't serialize a null object, simply return the default for that object
        if (ReferenceEquals(source, null))
            return default;

        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(source));
    }
}
