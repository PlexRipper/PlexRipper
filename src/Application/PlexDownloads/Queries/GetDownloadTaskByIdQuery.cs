using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class GetDownloadTaskByIdQuery : IRequest<Result<DownloadTask>>
    {
        public GetDownloadTaskByIdQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }
}