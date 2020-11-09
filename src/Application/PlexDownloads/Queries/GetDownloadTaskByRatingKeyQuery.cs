using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexDownloads
{
    public class GetDownloadTaskByRatingKeyQuery : IRequest<Result<DownloadTask>>
    {
        public GetDownloadTaskByRatingKeyQuery(int ratingKey, bool includeServer = false)
        {
            RatingKey = ratingKey;
            IncludeServer = includeServer;
        }

        public int RatingKey { get; }

        public bool IncludeServer { get; }
    }
}