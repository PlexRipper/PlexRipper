using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetAllDownloadTasksQuery : IRequest<Result<List<DownloadTask>>> { }