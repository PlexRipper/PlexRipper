using System.Diagnostics.CodeAnalysis;

namespace Reaparr.Application.Contracts;

public record CreateDownloadTasksRequest
{
    [SetsRequiredMembers]
    public CreateDownloadTasksRequest(
        List<DownloadMediaDTO> downloadMedias,
        int? destinationFolderPathId = null,
        string customDestinationFolderPath = ""
    )
    {
        DownloadMedias = downloadMedias;
        DestinationFolderPathId = destinationFolderPathId;
        CustomDestinationFolderPath = customDestinationFolderPath;
    }

    public required List<DownloadMediaDTO> DownloadMedias { get; init; } = [];

    public required int? DestinationFolderPathId { get; init; }

    public required string CustomDestinationFolderPath { get; init; }
}
