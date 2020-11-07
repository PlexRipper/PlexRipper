using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.FolderPaths.Commands;
using PlexRipper.Application.FolderPaths.Queries;
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

        public async Task<Result> CheckIfFolderPathsAreValid()
        {
            var folderPaths = await GetAllFolderPathsAsync();
            if (folderPaths.IsFailed)
            {
                return folderPaths.ToResult();
            }

            var errors = new List<Error>();
            folderPaths.Value.ForEach(folderPath =>
            {
                if (!folderPath.IsValid())
                {
                    errors.Add(new Error($"The {folderPath.DisplayName} is not a valid directory"));
                }
            });

            return errors.Count > 0 ? new Result().WithErrors(errors) : Result.Ok();
        }
    }
}