using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PlexRipper.WebAPI;

/// <summary>
/// Ensures all properties are "nullable: false", this prevents object | null when generating typescript in the front-end.
/// Source: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1346#issuecomment-551438503
/// </summary>
public class RequiredNotNullableSchemaFilter : ISchemaFilter {
    public void Apply(OpenApiSchema schema, SchemaFilterContext context) {
        if (schema.Properties == null) {
            return;
        }

        var requiredButNullableProperties = schema
            .Properties
            .Where(x => x.Value.Nullable && schema.Required.Contains(x.Key))
            .ToList();

        foreach (var property in requiredButNullableProperties) {
            property.Value.Nullable = false;
        }
    }
}