using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using Reaparr.Application.Contracts;
using Reaparr.BackgroundJobs.Contracts;
using Reaparr.SignalR.Contracts;

namespace Reaparr.AppHost;

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
            typeof(RefreshDataType),
            typeof(DownloadActions),
            typeof(NotificationDTO),
            typeof(ServerDownloadProgressDTO),
            typeof(DownloadProgressDTO),
            typeof(ServerDownloadProgressMessagePackDTO),
            typeof(DownloadProgressMessagePackDTO),
            typeof(ServerConnectionCheckStatusProgressDTO),
            // Background job updates
            typeof(CheckAllConnectionStatusUpdateDTO),
            typeof(DownloadJobUpdateDTO),
            typeof(InspectPlexServerJobUpdateDTO),
            typeof(MoveDownloadFileJobUpdateDTO),
            typeof(LibrarySyncJobQueueDTO),
            typeof(LibrarySyncProgressDTO),
            typeof(DownloadPatchDTO),
            typeof(DownloadPatchMessagePackDTO),
            typeof(AppUpdateDownloadProgressDTO),
        ];

        foreach (var type in types.Where(type => !context.SchemaResolver.HasSchema(type, false)))
            context.SchemaGenerator.Generate(type, context.SchemaResolver);
    }
}
