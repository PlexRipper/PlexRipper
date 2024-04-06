namespace PlexRipper.Domain;

public class DownloadTaskTvShowEpisode : DownloadTaskParentBase
{
    #region Relationships

    public required List<DownloadTaskTvShowEpisodeFile> Children { get; set; } = new();

    public required Guid ParentId { get; set; }

    public required DownloadTaskTvShowSeason Parent { get; set; }

    #endregion

    #region Helpers

    public override PlexMediaType MediaType => PlexMediaType.Episode;

    public override DownloadTaskType DownloadTaskType => DownloadTaskType.Episode;

    public override bool IsDownloadable => false;

    public override int Count => Children.Sum(x => x.Count) + 1;

    public override DownloadTaskKey ToParentKey() => new(DownloadTaskType.Season, ParentId, PlexServerId, PlexLibraryId);

    #endregion
}