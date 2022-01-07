using System;
using System.Collections.Generic;
using System.Linq;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using PlexRipper.Application;
using PlexRipper.Domain;
using PlexRipper.DownloadManager;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.SignalR.Common;

namespace PlexRipper.WebAPI.Config
{
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
                typeof(DownloadTaskCreationProgress),
                typeof(LibraryProgress),
                typeof(InspectServerProgress),
                typeof(FileMergeProgress),
                typeof(NotificationDTO),
                typeof(SyncServerProgress),
                typeof(DownloadProgressDTO),
                typeof(ServerDownloadProgressDTO),
            };

            foreach (Type type in types.Where(type => !context.SchemaResolver.HasSchema(type, false)))
            {
                context.SchemaGenerator.Generate(type, context.SchemaResolver);
            }
        }
    }
}