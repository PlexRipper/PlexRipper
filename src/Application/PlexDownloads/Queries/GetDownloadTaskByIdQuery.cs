namespace PlexRipper.Application;

public class GetDownloadTaskByIdQuery : IRequest<Result<DownloadTask>>
{
    public GetDownloadTaskByIdQuery(int id, bool includeChildren = false)
    {
        Id = id;
        IncludeChildren = includeChildren;
    }

    public int Id { get; }

    public bool IncludeChildren { get; }
}