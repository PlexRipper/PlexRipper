using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Domain.Config;

namespace PlexRipper.Data.Common;

/// <summary>
/// Extensions for <see cref="T:Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder" />.
/// </summary>
public static class PropertyBuilderExtensions
{
    /// <summary>
    /// Serializes field as JSON blob in database.
    /// </summary>
    public static PropertyBuilder<T> HasJsonValueConversion<T>(
        this PropertyBuilder<T> propertyBuilder)
        where T : class
    {
        // TODO Could add JsonSchema Source Generators here to speed things up
        propertyBuilder.HasConversion(
            v => JsonSerializer.Serialize(v, DefaultJsonSerializerOptions.ConfigBase),
            v => JsonSerializer.Deserialize<T>(v, DefaultJsonSerializerOptions.ConfigBase));
        return propertyBuilder;
    }
}