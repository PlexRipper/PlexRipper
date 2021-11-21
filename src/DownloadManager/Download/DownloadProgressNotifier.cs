using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application;
using PlexRipper.Application.Common;
using PlexRipper.Domain.Converters;

namespace PlexRipper.DownloadManager
{
    public class DownloadProgressNotifier : IDownloadProgressNotifier
    {
        private readonly IMediator _mediator;

        private readonly ISignalRService _signalRService;

        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            ReferenceHandler = ReferenceHandler.Preserve,
        };

        public DownloadProgressNotifier(IMediator mediator, ISignalRService signalRService)
        {
            _mediator = mediator;
            _signalRService = signalRService;
        }

        public async Task<Result<string>> SendDownloadProgress(int plexServerId)
        {
            var downloadTasksResult = await _mediator.Send(new GetAllDownloadTasksInPlexServerByIdQuery(plexServerId));
            if (downloadTasksResult.IsSuccess)
            {
                await _signalRService.SendDownloadProgressUpdate(plexServerId, downloadTasksResult.Value);
            }

            try
            {
                // Create hash to see if there are any changes in the updates
                var jsonString = JsonSerializer.Serialize(downloadTasksResult.Value, _jsonSerializerOptions);
                return Result.Ok(HashGenerator.CreateMD5(jsonString));
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e)).LogError();
            }
        }
    }
}