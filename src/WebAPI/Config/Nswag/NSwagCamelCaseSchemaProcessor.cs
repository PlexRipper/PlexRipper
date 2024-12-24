using NJsonSchema;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

public class NSwagCamelCaseSchemaProcessor : IDocumentProcessor
{
    public void Process(DocumentProcessorContext context)
    {
        var schemas = context.Document.Components.Schemas;

        foreach (var schema in schemas)
        {
            // Process each schema's properties
            var properties = schema.Value.Properties;

            if (properties != null)
            {
                var newProperties = new Dictionary<string, JsonSchemaProperty>();

                foreach (var property in properties)
                {
                    // Convert snake-case to camelCase
                    var camelCaseName = ConvertSnakeToCamelCase(property.Key);
                    newProperties[camelCaseName] = property.Value;
                }

                // Replace schema properties with camelCase versions
                properties.Clear();
                foreach (var prop in newProperties)
                {
                    properties.Add(prop.Key, prop.Value);
                }
            }
        }
    }

    private string ConvertSnakeToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var segments = input.Split('-');
        if (segments.Length == 1)
            return char.ToLowerInvariant(input[0]) + input.Substring(1);

        return segments[0] + string.Concat(segments[1..].Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1)));
    }
}
