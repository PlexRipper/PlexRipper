using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
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

        public async Task<Result<FolderPath>> CreateFolderPath(FolderPath folderPath)
        {
            var folderPathId = await _mediator.Send(new CreateFolderPathCommand(folderPath));
            if (folderPathId.IsFailed)
            {
                return folderPathId.ToResult();
            }

            return await _mediator.Send(new GetFolderPathByIdQuery(folderPathId.Value));
        }

        public Task<Result<FolderPath>> UpdateFolderPathAsync(FolderPath folderPath)
        {
            return _mediator.Send(new UpdateFolderPathCommand(folderPath));
        }

        /// <inheritdoc/>
        public async Task<Result<FolderPath>> GetDownloadFolderAsync()
        {
            return await _mediator.Send(new GetFolderPathByIdQuery(1));
        }

        /// <inheritdoc/>
        public async Task<Result<FolderPath>> GetMovieDestinationFolderAsync()
        {
            return await _mediator.Send(new GetFolderPathByIdQuery(2));
        }

        /// <inheritdoc/>
        public async Task<Result<FolderPath>> GetTvShowDestinationFolderAsync()
        {
            return await _mediator.Send(new GetFolderPathByIdQuery(3));
        }

        public async Task<Result<FolderPath>> GetDestinationFolderByMediaType(PlexMediaType mediaType)
        {
            switch (mediaType)
            {
                case PlexMediaType.Movie:
                    return await GetMovieDestinationFolderAsync();
                case PlexMediaType.TvShow:
                case PlexMediaType.Season:
                case PlexMediaType.Episode:
                    return await GetTvShowDestinationFolderAsync();
                default:
                    return await GetDownloadFolderAsync();
            }
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