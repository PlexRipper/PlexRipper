using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlexRipper.Domain
{
public class FolderTypeConverter : JsonConverter<FolderType>
{
    public override FolderType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString().ToFolderType();
    }

    public override void Write(Utf8JsonWriter writer, FolderType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToFolderTypeString());
    }
}
}