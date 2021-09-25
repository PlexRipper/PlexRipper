using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlexRipper.Domain
{
    public class FileSystemEntityTypeConverter: JsonConverter<FileSystemEntityType>
    {
        public override FileSystemEntityType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString().ToFileSystemEntityType();
        }

        public override void Write(Utf8JsonWriter writer, FileSystemEntityType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToFileSystemEntityTypeString());
        }
    }
}