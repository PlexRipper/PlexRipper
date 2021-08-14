using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlexRipper.Settings.Config
{
    /// <summary>
    /// Enables conversion from interfaces to concrete implementation during serialization and deserialization.
    /// Source: https://stackoverflow.com/a/64636093/8205497
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    public class TypeMappingConverter<TType, TImplementation> : JsonConverter<TType> where TImplementation : TType
    {
        [return: MaybeNull]
        public override TType Read(
            ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<TImplementation>(ref reader, options);
        }

        public override void Write(
            Utf8JsonWriter writer, TType value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (TImplementation)value!, options);
        }
    }
}