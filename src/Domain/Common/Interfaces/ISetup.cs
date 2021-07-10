using System.Threading.Tasks;
using FluentResults;

namespace PlexRipper.Domain
{
    public interface ISetup
    {
        public Task<Result> SetupAsync();
    }
}