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
            options.PropertyNamingPolicy = null;
            return options;
        });

    public static JsonSerializerOptions ConfigStandard => ConfigStandardField.Value;
    public static JsonSerializerOptions ConfigCapitalized => ConfigCapitalizedField.Value;

    public static JsonSerializerOptions ConfigIndented => ConfigIndentedField.Value;
    public static JsonSerializerOptions ConfigManagerOptions => ConfigManagerOptionsField.Value;
}
