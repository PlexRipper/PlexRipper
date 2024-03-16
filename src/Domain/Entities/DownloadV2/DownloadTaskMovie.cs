using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class DownloadTaskMovie : DownloadTaskParentBase
{
    #region Relationships

    public List<DownloadTaskMovieFile> Children { get; set; } = new();

    #endregion

    #region Helpers

    public override void SetNull()
    {
        base.SetNull();
        Children = null;
    }

    [NotMapped]
    public override PlexMediaType MediaType => PlexMediaType.Movie;

    [NotMapped]
    public override DownloadTaskType DownloadTaskType => DownloadTaskType.Movie;

    [NotMapped]
    public override bool IsDownloadable => false;

    [NotMapped]
    public override int Count => Children.Sum(x => x.Count) + 1;

    #endregion
}