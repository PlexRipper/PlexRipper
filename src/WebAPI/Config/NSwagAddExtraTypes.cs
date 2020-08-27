using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using PlexRipper.Domain.Types;
using PlexRipper.DownloadManager.Common;
using PlexRipper.SignalR.Common;

namespace PlexRipper.WebAPI.Config
{
    public class NSwagAddExtraTypes : IDocumentProcessor
    {
        public void Process(DocumentProcessorContext context)
        {
            var types = new List<Type>
            {
                typeof(DownloadProgress),
                typeof(DownloadTaskCreationProgress),
                typeof(LibraryProgress),
                typeof(DownloadStatusChanged),
            };

            foreach (var type in types)
            {
                if (!context.SchemaResolver.HasSchema(type, false))
                {
                    context.SchemaGenerator.Generate(type, context.SchemaResolver);
                }
            }
        }
    }
}