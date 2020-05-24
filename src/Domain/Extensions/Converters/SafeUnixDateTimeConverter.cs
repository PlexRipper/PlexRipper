#nullable enable
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace PlexRipper.Domain.Extensions.Converters
{
    /// <summary>
    /// Huge numbers might be returned, which is why the type in the DTO is a string, which will be safely converted here to a valid DateTime.
    /// </summary>
    public class SafeUnixDateTimeConverter : UnixDateTimeConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            try
            {
                base.WriteJson(writer, value, serializer);
            }
#pragma warning disable 168
            catch (Exception e)
#pragma warning restore 168
            {
                // Console.WriteLine(e);
            }
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            try
            {
                return base.ReadJson(reader, objectType, existingValue, serializer);
            }
#pragma warning disable 168
            catch (Exception e)
#pragma warning restore 168
            {
                // Console.WriteLine(e);
            }

            return DateTime.MinValue;
        }
    }
}
