using Application.Contracts;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using WebAPI.Contracts;

namespace PlexRipper.WebAPI;

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
            typeof(DownloadTaskCreationProgress),
            typeof(LibraryProgress),
            typeof(InspectServerProgressDTO),
            typeof(FileMergeProgress),
            typeof(NotificationDTO),
            typeof(SyncServerMediaProgress),
            typeof(DownloadProgressDTO),
            typeof(ServerDownloadProgressDTO),
            typeof(ServerConnectionCheckStatusProgressDTO),
            // Job status updates
            typeof(JobStatusUpdateDTO<object>),
            typeof(CheckAllConnectionStatusUpdateDTO),
        ];

        foreach (var type in types.Where(type => !context.SchemaResolver.HasSchema(type, false)))
            context.SchemaGenerator.Generate(type, context.SchemaResolver);
    }
}
