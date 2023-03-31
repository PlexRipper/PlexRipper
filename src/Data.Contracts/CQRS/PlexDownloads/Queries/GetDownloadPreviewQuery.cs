using DownloadManager.Contracts;
using FluentResults;
using MediatR;

namespace Data.Contracts;

public class GetDownloadPreviewQuery : IRequest<Result<List<DownloadPreviewDTO>>>
{
    public List<DownloadMediaDTO> DownloadMedias { get; }

    public GetDownloadPreviewQuery(List<DownloadMediaDTO> downloadMedias)
    {
        DownloadMedias = downloadMedias;
    }
}