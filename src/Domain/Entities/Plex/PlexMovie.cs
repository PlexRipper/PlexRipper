using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

[Table("PlexMovie")]
public class PlexMovie : PlexMedia
{
    #region Relationships

    public List<PlexMovieGenre> PlexMovieGenres { get; init; }

    public List<PlexMovieRole> PlexMovieRoles { get; init; }

    #endregion

    #region Helpers

    [NotMapped]
    public List<PlexMediaDataPart> MovieParts => MovieData.SelectMany(x => x.Parts).ToList();

    [NotMapped]
    public List<PlexMediaData> MovieData => MediaData.MediaData ?? [];

    [NotMapped]
    public override PlexMediaType Type => PlexMediaType.Movie;

    #endregion
}
