using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace PlexRipper.Domain.Config;

public static class DefaultJsonSerializerOptions
{
    /// <summary>
    /// Disable throwing exceptions when a required property is missing, this will be defaulted to the default value.
    /// Otherwise, we need separate models for serialization/deserialization and DTO's.
    /// Source: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/required-properties
    /// </summary>
    private static readonly DefaultJsonTypeInfoResolver DisableThrowingExceptionsWhenRequiredPropertyIsMissing =
        new()
        {
            Modifiers =
            {
                static typeInfo =>
                {
                    if (typeInfo.Kind != JsonTypeInfoKind.Object)
                        return;

                    foreach (var propertyInfo in typeInfo.Properties)
                        // Strip IsRequired constraint from every property.
                        propertyInfo.IsRequired = false;
                },
            },
        };

    /// <summary>
    /// If a property is missing, and would normally set null, then skip setting the property.
    /// Source: https://stackoverflow.com/a/77520195/8205497
    /// </summary>
    /// <param name="typeInfo"></param>
    private static void InterceptNullSetter(JsonTypeInfo typeInfo)
    {
        foreach (var propertyInfo in typeInfo.Properties)
        {
            var setProperty = propertyInfo.Set;
            if (setProperty is not null)
            {
                propertyInfo.Set = (obj, value) =>
                {
                    if (value != null)
                    {
                        //value = ((Demo)obj).B + value;
                        setProperty(obj, value);
                    }
                };
            }
        }
    }

    private static JsonSerializerOptions CreateBaseOptions() =>
        new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            TypeInfoResolver = DisableThrowingExceptionsWhenRequiredPropertyIsMissing,
        };

    private static readonly Lazy<JsonSerializerOptions> ConfigStandardField =
        new(() =>
        {
            var options = CreateBaseOptions();
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            return options;
        });

    private static readonly Lazy<JsonSerializerOptions> ConfigCapitalizedField =
        new(() =>
        {
            var options = CreateBaseOptions();
            options.PropertyNamingPolicy = null;
            return options;
        });

    private static readonly Lazy<JsonSerializerOptions> ConfigIndentedField =
        new(() =>
        {
            var options = CreateBaseOptions();
            options.WriteIndented = true;
            return options;
        });

    private static readonly Lazy<JsonSerializerOptions> ConfigManagerOptionsField =
        new(() =>
        {
            var options = ConfigIndentedField.Value;

            //If Model.property is collect and this config is Populate,then skip propertyInfo.Set
            options.PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate;
            options.TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { InterceptNullSetter } };
            options.WriteIndented = true;
            options.PropertyNamingPolicy = null;
            return options;
        });

    public static JsonSerializerOptions ConfigStandard => ConfigStandardField.Value;
    public static JsonSerializerOptions ConfigCapitalized => ConfigCapitalizedField.Value;

    public static JsonSerializerOptions ConfigIndented => ConfigIndentedField.Value;
    public static JsonSerializerOptions ConfigManagerOptions => ConfigManagerOptionsField.Value;
}
