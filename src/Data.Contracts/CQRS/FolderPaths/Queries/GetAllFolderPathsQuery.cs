using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetAllFolderPathsQuery : IRequest<Result<List<FolderPath>>> { }