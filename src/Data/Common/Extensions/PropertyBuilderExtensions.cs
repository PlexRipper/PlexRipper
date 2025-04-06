using System.Text.Json;
using Logging.Interface;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Common;

/// <summary>
/// Extensions for <see cref="T:Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder" />.
/// </summary>
public static class PropertyBuilderExtensions
{
    private static ILog _log = LogManager.CreateLogInstance(typeof(PropertyBuilderExtensions));

    /// <summary>
    /// Serializes field as JSON blob in database.
    /// </summary>
    public static PropertyBuilder<T> HasJsonValueConversion<T>(this PropertyBuilder<T> propertyBuilder)
        where T : class
    {
        try
        {
            // TODO:Could add JsonSchema Source Generators here to speed things up
            propertyBuilder.HasConversion(
                v => JsonSerializer.Serialize(v, DefaultJsonSerializerOptions.ConfigStandard),
                v => JsonSerializer.Deserialize<T>(v, DefaultJsonSerializerOptions.ConfigStandard)!
            );
        }
        catch (Exception e)
        {
            _log.Error(e);
            throw;
        }

        return propertyBuilder;
    }
}
