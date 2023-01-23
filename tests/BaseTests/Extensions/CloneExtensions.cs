using Newtonsoft.Json;

namespace PlexRipper.BaseTests;

public static class CloneExtensions
{
    public static T CloneJson<T>(this T source)
    {
        // Don't serialize a null object, simply return the default for that object
        if (Object.ReferenceEquals(source, null))
        {
            return default(T);
        }

        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source));
    }
}