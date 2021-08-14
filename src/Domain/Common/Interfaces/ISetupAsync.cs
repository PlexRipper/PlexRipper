using System.Threading.Tasks;
using FluentResults;

namespace PlexRipper.Domain
{
    public interface ISetupAsync
    {
        public Task<Result> SetupAsync();
    }
}