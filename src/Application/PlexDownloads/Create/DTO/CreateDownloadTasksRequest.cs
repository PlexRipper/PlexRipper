using System.Diagnostics.CodeAnalysis;
using Application.Contracts;

namespace PlexRipper.Application;

public record CreateDownloadTasksRequest
{
    [SetsRequiredMembers]
    public CreateDownloadTasksRequest(List<DownloadMediaDTO> tvShows, int destinationFolderPathId = 0)
    {
        DownloadMedias = tvShows;
        DestinationFolderPathId = destinationFolderPathId;
    }

    public required int DestinationFolderPathId { get; init; }

    public required List<DownloadMediaDTO> DownloadMedias { get; init; } = [];
}
