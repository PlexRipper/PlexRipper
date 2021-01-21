using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface IPlexMediaService
    {
        Task<Result<byte[]>> GetThumbnailImage(int mediaId, PlexMediaType mediaType, int width = 0, int height = 0);

        Task<Result<byte[]>> GetBannerImage(int mediaId, PlexMediaType mediaType, int width = 0, int height = 0);
    }
}