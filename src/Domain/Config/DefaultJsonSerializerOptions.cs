﻿using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace PlexRipper.Domain.Config;

public static class DefaultJsonSerializerOptions
{
    /// <summary>
    /// Disable throwing exceptions when a required property is missing, this will be defaulted to the default value.
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

    private static JsonSerializerOptions ConfigBase { get; } =
        new()
        {
            // PropertyNameCaseInsensitive is crucial otherwise empty objects are created with no values
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            TypeInfoResolver = DisableThrowingExceptionsWhenRequiredPropertyIsMissing,
        };

    public static JsonSerializerOptions ConfigStandard
    {
        get
        {
            var options = ConfigBase;
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            return new JsonSerializerOptions(options);
        }
    }

    public static JsonSerializerOptions ConfigCapitalized
    {
        get
        {
            var options = ConfigBase;

            // PropertyNamingPolicy is null which will save as CapitalizedCaseForProperties
            options.PropertyNamingPolicy = null;
            return new JsonSerializerOptions(options);
        }
    }

    public static JsonSerializerOptions ConfigIndented { get; } =
        new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
            WriteIndented = true,
            TypeInfoResolver = DisableThrowingExceptionsWhenRequiredPropertyIsMissing,
        };

    public static JsonSerializerOptions ConfigManagerOptions => ConfigIndented;
}
