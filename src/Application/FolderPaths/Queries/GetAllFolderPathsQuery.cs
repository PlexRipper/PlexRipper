using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.FolderPaths
{
    public class GetAllFolderPathsQuery : IRequest<Result<List<FolderPath>>> { }
}