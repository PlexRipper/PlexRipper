using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

[Table("PlexMovie")]
public class PlexMovie : PlexMedia
{
    #region Relationships

    public List<PlexMovieGenre> PlexMovieGenres { get; set; }

    public List<PlexMovieRole> PlexMovieRoles { get; set; }

    #endregion

    #region Helpers

    [NotMapped]
    public List<PlexMediaDataPart> MovieParts => MovieData.SelectMany(x => x.Parts).ToList();

    [NotMapped]
    public List<PlexMediaData> MovieData => MediaData.MediaData ?? new List<PlexMediaData>();

    [NotMapped]
    public override PlexMediaType Type => PlexMediaType.Movie;

    #endregion
}