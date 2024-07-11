#nullable enable
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class DownloadTaskMovie : DownloadTaskParentBase
{
    #region Relationships

    public required List<DownloadTaskMovieFile> Children { get; set; } = new();

    #endregion

    #region Helpers

    [NotMapped]
    public override PlexMediaType MediaType => PlexMediaType.Movie;

    [NotMapped]
    public override DownloadTaskType DownloadTaskType => DownloadTaskType.Movie;

    [NotMapped]
    public override bool IsDownloadable => false;

    [NotMapped]
    public override int Count => Children.Sum(x => x.Count) + 1;

    public override DownloadTaskKey? ToParentKey() => null;

    #endregion
}
