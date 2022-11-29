using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

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
// @formatter:off
        propertyBuilder.HasConversion(
            v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore }),
            v => JsonConvert.DeserializeObject<T>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore }));
// @formatter:on
        return propertyBuilder;
    }
}