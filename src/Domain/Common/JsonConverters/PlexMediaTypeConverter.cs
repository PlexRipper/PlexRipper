using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlexRipper.Domain;

public class PlexMediaTypeConverter : JsonConverter<PlexMediaType>
{
    public override PlexMediaType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString().ToPlexMediaType();
    }

    public override void Write(Utf8JsonWriter writer, PlexMediaType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToPlexMediaTypeString());
    }
}