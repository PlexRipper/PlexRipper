namespace PlexRipper.Application
{
    public interface IPlexMediaService
    {
        Task<Result<byte[]>> GetThumbnailImage(int mediaId, PlexMediaType mediaType, int width = 0, int height = 0);

        Task<Result<byte[]>> GetBannerImage(int mediaId, PlexMediaType mediaType, int width = 0, int height = 0);
    }
}