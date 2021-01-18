using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface IPlexTvShowService
    {
        Task<Result<PlexTvShow>> GetTvShow(int id);
    }
}