using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlexRipper.Domain
{
    public class DownloadStatusConverter : JsonConverter<DownloadStatus>
    {
        public override DownloadStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString().ToDownloadStatus();
        }

        public override void Write(Utf8JsonWriter writer, DownloadStatus value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToDownloadStatusString());
        }
    }
}