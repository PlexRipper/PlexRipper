namespace PlexRipper.Domain;

public class DownloadTaskTvShowEpisodeFile : DownloadTaskFileBase
{
    #region Relationships

    public required DownloadTaskTvShowEpisode? Parent { get; init; }

    public required Guid ParentId { get; init; }

    #endregion

    #region Helpers

    public override PlexMediaType MediaType => PlexMediaType.Episode;

    public override DownloadTaskType DownloadTaskType => DownloadTaskType.EpisodeData;

    public override bool IsDownloadable => true;

    public override int Count => 1;

    public override DownloadTaskKey ToParentKey() =>
        new()
        {
            Type = DownloadTaskType.Episode,
            Id = ParentId,
            PlexServerId = PlexServerId,
            PlexLibraryId = PlexLibraryId,
        };

    #endregion
}
