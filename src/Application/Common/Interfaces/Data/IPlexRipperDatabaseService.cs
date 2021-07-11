using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface IPlexRipperDatabaseService : ISetup
    {
        Result BackUpDatabase();

        Task<Result> ResetDatabase();
    }
}