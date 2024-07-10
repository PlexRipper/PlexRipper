using System.Buffers;
using System.Buffers.Text;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Helpers;

public class LongValueConverter : JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            // The minimum long value UnixTime seconds that can be used to parse to DateTime.MinValue
            var value = -62135596800;

            // try to parse number directly from bytes
            var span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;

            // span.Length
            if (
                Utf8Parser.TryParse(span, out long number, out var bytesConsumed)
                && span.Length == bytesConsumed
            )
            {
                if (value >= -62135596800 && value <= 253402300799)
                    return number;
            }

            // try to parse from a string if the above failed, this covers cases with other escaped/UTF characters
            if (long.TryParse(reader.GetString(), out number))
            {
                if (value >= -62135596800 && value <= 253402300799)
                    return number;
            }
        }

        // fallback to default handling
        return -1;
    }

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

public class DoubleValueConverter : JsonConverter<double>
{
    public override double Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            // try to parse number directly from bytes
            var span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
            if (
                Utf8Parser.TryParse(span, out double number, out var bytesConsumed)
                && span.Length == bytesConsumed
            )
                return number;

            // try to parse from a string if the above failed, this covers cases with other escaped/UTF characters
            if (double.TryParse(reader.GetString(), out number))
                return number;
        }

        // fallback to default handling
        return reader.GetDouble();
    }

    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(CultureInfo.DefaultThreadCurrentCulture));
    }
}

public class IntValueConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            // try to parse number directly from bytes
            var span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
            if (
                Utf8Parser.TryParse(span, out int number, out var bytesConsumed)
                && span.Length == bytesConsumed
            )
                return number;

            // try to parse from a string if the above failed, this covers cases with other escaped/UTF characters
            if (int.TryParse(reader.GetString(), out number))
                return number;
        }

        // fallback to default handling
        return reader.GetInt32();
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

public class BooleanValueConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            if (
                string.Equals("1", reader.GetString(), StringComparison.OrdinalIgnoreCase)
                || string.Equals("0", reader.GetString(), StringComparison.OrdinalIgnoreCase)
            )
                return Convert.ToBoolean(Convert.ToInt16(reader.GetString()));

            // try to parse number directly from bytes
            var span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
            if (
                Utf8Parser.TryParse(span, out bool boolean, out var bytesConsumed)
                && span.Length == bytesConsumed
            )
                return boolean;

            // try to parse from a string if the above failed, this covers cases with other escaped/UTF characters
            if (bool.TryParse(reader.GetString(), out boolean))
                return boolean;
        }

        // fallback to default handling
        return reader.GetBoolean();
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
