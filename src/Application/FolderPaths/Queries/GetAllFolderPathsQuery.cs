using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.FolderPaths.Queries
{
    public class GetAllFolderPathsQuery : IRequest<Result<List<FolderPath>>> { }
}