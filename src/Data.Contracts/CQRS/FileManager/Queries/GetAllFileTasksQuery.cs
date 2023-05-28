using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetAllFileTasksQuery : IRequest<Result<List<DownloadFileTask>>> { }