using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetPlexMediaDataByLibraryIdQuery : IRequest<Result<List<PlexMediaSlim>>>
{
    public int LibraryId { get; }
    public int Page { get; }
    public int PageSize { get; }

    public GetPlexMediaDataByLibraryIdQuery(int libraryId, int page = 0, int pageSize = 100)
    {
        LibraryId = libraryId;
        Page = page;
        PageSize = pageSize;
    }
}