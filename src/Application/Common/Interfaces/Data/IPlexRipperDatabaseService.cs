using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public interface IPlexRipperDatabaseService : ISetupAsync
    {
        Result BackUpDatabase();

        Task<Result> ResetDatabase();
    }
}