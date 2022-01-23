using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlexRipper.Domain
{
    public class NotificationLevelConverter : JsonConverter<NotificationLevel>
    {
        public override NotificationLevel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString().ToNotificationLevel();
        }

        public override void Write(Utf8JsonWriter writer, NotificationLevel value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToNotificationLevelString());
        }
    }
}