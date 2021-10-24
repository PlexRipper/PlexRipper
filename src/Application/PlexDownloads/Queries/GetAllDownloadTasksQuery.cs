using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class GetAllDownloadTasksQuery : IRequest<Result<List<DownloadTask>>> { }
}