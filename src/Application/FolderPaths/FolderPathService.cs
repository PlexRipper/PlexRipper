using FluentResults;
using MediatR;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.FolderPaths.Queries;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    }
}
