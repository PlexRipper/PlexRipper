using Application.Contracts;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using WebAPI.Contracts;

namespace PlexRipper.WebAPI;

/// <summary>
///  Adds extra types to the Swagger client that are not automatically added.
/// </summary>
public class NSwagAddExtraTypes : IDocumentProcessor
{
    /// <summary>
    /// Registers classes in the Swagger client.
    /// </summary>
    /// <param name="context">The <see cref="DocumentProcessorContext"/> used to register types in.</param>
    public void Process(DocumentProcessorContext context)
    {
        List<Type> types =
        [
            typeof(MessageTypes),
            typeof(JobTypes),
            typeof(JobStatus),
            typeof(DataType),
            typeof(DownloadActions),
            typeof(LibraryProgress),
            typeof(NotificationDTO),
            typeof(SyncServerMediaProgress),
            typeof(DownloadProgressDTO),
            typeof(ServerDownloadProgressDTO),
            typeof(ServerConnectionCheckStatusProgressDTO),
            // Background job updates
            typeof(CheckAllConnectionStatusUpdateDTO),
            typeof(DownloadJobUpdateDTO),
            typeof(SyncServerMediaJobUpdateDTO),
            typeof(InspectPlexServerJobUpdateDTO),
            typeof(FileMergeJobUpdateDTO),
        ];

        foreach (var type in types.Where(type => !context.SchemaResolver.HasSchema(type, false)))
            context.SchemaGenerator.Generate(type, context.SchemaResolver);
    }
}
