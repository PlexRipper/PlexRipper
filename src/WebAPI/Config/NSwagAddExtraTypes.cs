using System;
using System.Collections.Generic;
using System.Linq;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using PlexRipper.Domain;
using PlexRipper.DownloadManager.Common;
using PlexRipper.SignalR.Common;

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
            List<Type> types = new List<Type>
            {
                typeof(DownloadProgress),
                typeof(DownloadTaskCreationProgress),
                typeof(LibraryProgress),
                typeof(DownloadStatusChanged),
                typeof(FileMergeProgress),
                typeof(NotificationUpdate),
            };

            foreach (Type type in types.Where(type => !context.SchemaResolver.HasSchema(type, false)))
            {
                context.SchemaGenerator.Generate(type, context.SchemaResolver);
            }
        }
    }
}