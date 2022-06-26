using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexAccounts
{
    public class GetAllPlexAccountsQuery : IRequest<Result<List<PlexAccount>>>
    {
        public GetAllPlexAccountsQuery(bool includePlexServers = false, bool includePlexLibraries = false, bool onlyEnabled = false)
        {
            IncludePlexServers = includePlexServers;
            IncludePlexLibraries = includePlexLibraries;
            OnlyEnabled = onlyEnabled;
        }

        public bool IncludePlexServers { get; }

        public bool IncludePlexLibraries { get; }

        public bool OnlyEnabled { get; }
    }
}