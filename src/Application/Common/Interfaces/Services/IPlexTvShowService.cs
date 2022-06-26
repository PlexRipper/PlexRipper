using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public interface IPlexTvShowService : IPlexMediaService
    {
        Task<Result<PlexTvShow>> GetTvShow(int id);
    }
}