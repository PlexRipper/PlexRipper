namespace PlexRipper.Application.PlexAccounts
{
    /// <summary>
    ///     Returns the <see cref="PlexAccount" /> by its id without any includes.
    /// </summary>
    public class GetPlexAccountByUsernameQuery : IRequest<Result<PlexAccount>>
    {
        public GetPlexAccountByUsernameQuery(string username, bool includePlexServers = false, bool includePlexLibraries = false)
        {
            Username = username;
            IncludePlexServers = includePlexServers;
            IncludePlexLibraries = includePlexLibraries;
        }

        public string Username { get; }

        public bool IncludePlexServers { get; }

        public bool IncludePlexLibraries { get; }
    }
}