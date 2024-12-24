using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace PlexRipper.WebAPI;

/// <summary>
/// This is a custom NSwag operation processor that converts query parameters to camel case.
/// NOTE: This is a workaround for the NSwag bug that does not convert query parameters to camel case but makes them capitalized. Breaking the API contract naming convention.
/// </summary>
public class NSwagQueryCamelCase : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        if (context.OperationDescription.Operation.Parameters != null)
        {
            foreach (var parameter in context.OperationDescription.Operation.Parameters)
            {
                if (string.IsNullOrEmpty(parameter.Name))
                    continue;

                parameter.Name = char.ToLowerInvariant(parameter.Name[0]) + parameter.Name.Substring(1);
            }
        }

        return true;
    }
}
