using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.FileManager.Queries
{
    public class GetAllFileTasksQuery : IRequest<Result<List<DownloadFileTask>>> { }
}