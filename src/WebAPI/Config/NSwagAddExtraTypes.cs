using BackgroundServices.Contracts;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using PlexRipper.DownloadManager;
using PlexRipper.WebAPI.Common;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.SignalR.Common;
using WebAPI.Contracts;

namespace PlexRipper.WebAPI;

/// <summary>
/// Swagger/NSwag only picks up types that are directly used in API results.
/// So this class is meant to register classes which can then be picked up by the Typescript DTO generator in the front-end.
/// </summary>
public class NSwagAddExtraTypes : IDocumentProcessor
{
    /// <summary>
    /// Registers classes in the Swagger client.
    /// </summary>
    /// <param name="context">The <see cref="DocumentProcessorContext"/> used to register types in.</param>
    public void Process(DocumentProcessorContext context)
    {
        List<Type> types = new()
        {
            typeof(MessageTypes),
            typeof(JobTypes),
            typeof(JobStatus),
            typeof(JobStatusUpdateDTO),
            typeof(DownloadTaskCreationProgress),
            typeof(LibraryProgress),
            typeof(InspectServerProgressDTO),
            typeof(FileMergeProgress),
            typeof(NotificationDTO),
            typeof(SyncServerProgress),
            typeof(DownloadProgressDTO),
            typeof(ServerDownloadProgressDTO),
            typeof(ServerConnectionCheckStatusProgressDTO),
        };

        foreach (var type in types.Where(type => !context.SchemaResolver.HasSchema(type, false)))
            context.SchemaGenerator.Generate(type, context.SchemaResolver);
    }
}