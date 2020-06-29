using System;
using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Config.Converters
{
    public class LongToDateTime : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                // The minimum long value UnixTime seconds that can be used to parse to DateTime.MinValue
                var value = -62135596800;

                // try to parse number directly from bytes
                ReadOnlySpan<byte> span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                if (Utf8Parser.TryParse(span, out long number, out int bytesConsumed) && span.Length == bytesConsumed)
                {
                    value = number;
                }

                // try to parse from a string if the above failed, this covers cases with other escaped/UTF characters
                if (Int64.TryParse(reader.GetString(), out number))
                {
                    value = number;
                }

                if (value >= -62135596800 && value <= 253402300799)
                {
                    return DateTimeOffset.FromUnixTimeSeconds(value).DateTime.ToUniversalTime();
                }
            }

            // fallback to default handling
            return DateTime.MinValue;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
