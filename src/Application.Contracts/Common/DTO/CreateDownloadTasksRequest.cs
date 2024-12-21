using System.Diagnostics.CodeAnalysis;

namespace Application.Contracts;

public record CreateDownloadTasksRequest
{
    [SetsRequiredMembers]
    public CreateDownloadTasksRequest(List<DownloadMediaDTO> tvShows, int? destinationFolderPathId = null)
    {
        DownloadMedias = tvShows;
        DestinationFolderPathId = destinationFolderPathId;
    }

    public required int? DestinationFolderPathId { get; init; }

    public required List<DownloadMediaDTO> DownloadMedias { get; init; } = [];
}
