using FluentResults;
using PlexRipper.Domain;

namespace Application.Contracts;

public interface IPlexTvShowService : IPlexMediaService
{
    Task<Result<PlexTvShow>> GetTvShow(int id);
}