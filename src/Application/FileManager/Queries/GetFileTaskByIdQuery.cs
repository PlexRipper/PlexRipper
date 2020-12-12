using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.FileManager.Queries
{
    public class GetFileTaskByIdQuery : IRequest<Result<DownloadFileTask>>
    {
        public GetFileTaskByIdQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }
}