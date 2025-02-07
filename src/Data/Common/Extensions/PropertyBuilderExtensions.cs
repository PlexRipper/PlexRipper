using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Common;

/// <summary>
/// Extensions for <see cref="T:Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder" />.
/// </summary>
public static class PropertyBuilderExtensions
{
    /// <summary>
    /// Serializes field as JSON blob in database.
    /// </summary>
    public static PropertyBuilder<T> HasJsonValueConversion<T>(this PropertyBuilder<T> propertyBuilder)
        where T : class
    {
        // TODO:Could add JsonSchema Source Generators here to speed things up
        propertyBuilder.HasConversion(
            v => JsonSerializer.Serialize(v, DefaultJsonSerializerOptions.ConfigStandard),
            v => JsonSerializer.Deserialize<T>(v, DefaultJsonSerializerOptions.ConfigStandard)!
        );
        return propertyBuilder;
    }

    public static PropertyBuilder<List<T>> ListValueComparer<T>(this PropertyBuilder<List<T>> propertyBuilder)
        where T : class
    {
        propertyBuilder.Metadata.SetValueComparer(
            new ValueComparer<List<T>>(
                (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()
            )
        );
        return propertyBuilder;
    }
}
