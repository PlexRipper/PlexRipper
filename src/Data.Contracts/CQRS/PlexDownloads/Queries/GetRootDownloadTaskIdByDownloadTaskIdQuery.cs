using FluentResults;
using MediatR;

namespace Data.Contracts;

public class GetRootDownloadTaskIdByDownloadTaskIdQuery : IRequest<Result<int>>
{
    public GetRootDownloadTaskIdByDownloadTaskIdQuery(int id)
    {
        Id = id;
    }

    public int Id { get; }
}