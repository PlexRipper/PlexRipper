using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class GetAllFolderPathsQuery : IRequest<Result<List<FolderPath>>> { }
}