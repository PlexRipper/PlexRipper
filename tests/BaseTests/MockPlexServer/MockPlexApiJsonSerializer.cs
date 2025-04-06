using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PlexRipper.BaseTests;

public static class MockPlexApiJsonSerializer
{
    public static JsonSerializerSettings GetSettings()
    {
        var settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CustomContractResolver(),
            Converters = { new JsonPropertyEnumConverter() },
        };

        return settings;
    }
}

public class CustomContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        // Create the default property.
        var property = base.CreateProperty(member, memberSerialization);

        // Disable the required constraint so missing properties won't throw.
        property.Required = Required.Default;

        // Wrap the property's ValueProvider to intercept null assignments.
        if (property.Writable)
        {
            if (property.ValueProvider != null)
                property.ValueProvider = new NullSkippingValueProvider(property.ValueProvider);
        }

        // If the property type is an enum and doesn't already have a converter, assign our custom converter.
        if (property is { PropertyType.IsEnum: true, Converter: null })
        {
            property.Converter = new JsonPropertyEnumConverter();
        }

        return property;
    }
}

public class NullSkippingValueProvider : IValueProvider
{
    private readonly IValueProvider _innerProvider;

    public NullSkippingValueProvider(IValueProvider innerProvider)
    {
        _innerProvider = innerProvider;
    }

    public object? GetValue(object target) => _innerProvider.GetValue(target);

    public void SetValue(object target, object? value)
    {
        // Only set the property if value is not null.
        if (value != null)
        {
            _innerProvider.SetValue(target, value);
        }
    }
}

public class JsonPropertyEnumConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType.IsEnum;

    // We only implement serialization.
    public override bool CanRead => false;

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        // Get the enum type and member info for the current value.
        var enumType = value.GetType();
        var member = enumType.GetMember(value.ToString()!).FirstOrDefault();
        if (member != null)
        {
            // Look for the JsonProperty attribute on the enum member.
            var attr = member
                .GetCustomAttributes(typeof(JsonPropertyAttribute), false)
                .OfType<JsonPropertyAttribute>()
                .FirstOrDefault();
            if (attr != null)
            {
                // Write the attribute's PropertyName as a JSON string.
                writer.WriteValue(attr.PropertyName);
                return;
            }
        }

        // Fallback: write the enum's string representation.
        writer.WriteValue(value.ToString());
    }

    // Reading is not implemented in this converter.
    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object? existingValue,
        JsonSerializer serializer
    ) => throw new NotImplementedException("JsonPropertyEnumConverter only supports serialization.");
}
