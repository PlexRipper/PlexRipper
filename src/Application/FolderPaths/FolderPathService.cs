using FluentResults;
using MediatR;
using PlexRipper.Application.FolderPaths.Commands;
using PlexRipper.Application.FolderPaths.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRipper.Application.Common;
using PlexRipper.Domain;

namespace PlexRipper.Application.FolderPaths
{
    public class FolderPathService : IFolderPathService
    {
        private readonly IMediator _mediator;

        public FolderPathService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task<Result<List<FolderPath>>> GetAllFolderPathsAsync()
        {
            return _mediator.Send(new GetAllFolderPathsQuery());
        }

        public Task<Result<FolderPath>> UpdateFolderPathAsync(FolderPath folderPath)
        {
            return _mediator.Send(new UpdateFolderPathCommand(folderPath));
        }

        public async Task<Result<FolderPath>> GetDownloadFolderAsync()
        {
            // Get the download folder
            return await _mediator.Send(new GetFolderPathByIdQuery(1));
        }
    }
}