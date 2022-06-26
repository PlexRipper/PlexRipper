namespace PlexRipper.Application.PlexAccounts;

public class GetPlexAccountByIdQuery : IRequest<Result<PlexAccount>>
{
    /// <summary>
    ///     Returns the <see cref="PlexAccount" /> by its id with optional includes.
    /// </summary>
    public GetPlexAccountByIdQuery(int id, bool includePlexServers = false, bool includePlexLibraries = false)
    {
        Id = id;
        IncludePlexServers = includePlexServers;
        IncludePlexLibraries = includePlexLibraries;
    }

    public int Id { get; }

    public bool IncludePlexServers { get; }

    public bool IncludePlexLibraries { get; }
}