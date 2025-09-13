using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Reaparr.AppHost;

internal sealed class NSwagGlobalHeaders : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        // NOTE: See "EndpointExtensions.AddResponseHeaders" for globally added response headers
        foreach (var response in context.OperationDescription.Operation.Responses.Values)
        {
            response.Headers["X-Reaparr-Version"] = new OpenApiHeader
            {
                Kind = OpenApiParameterKind.Header,
                Example = "0.26.0-dev-2024-12-11",
                IsRequired = true,
                Description = "Current Reaparr version",
                Schema = new JsonSchema
                {
                    Type = JsonObjectType.String,
                    Example = "0.26.0-dev-2024-12-11",
                    Description = "Current Reaparr version",
                },
            };
        }

        return true;
    }
}
