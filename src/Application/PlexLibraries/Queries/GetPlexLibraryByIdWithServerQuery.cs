namespace PlexRipper.Application;

public class GetPlexLibraryByIdWithServerQuery : IRequest<Result<PlexLibrary>>
{
    public GetPlexLibraryByIdWithServerQuery(int id)
    {
        Id = id;
    }

    public int Id { get; }
}