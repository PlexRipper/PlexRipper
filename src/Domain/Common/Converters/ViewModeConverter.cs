using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlexRipper.Domain
{
    public class ViewModeConverter: JsonConverter<ViewMode>
    {
        public override ViewMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString().ToViewMode();
        }

        public override void Write(Utf8JsonWriter writer, ViewMode value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToViewModeString());
        }
    }
}