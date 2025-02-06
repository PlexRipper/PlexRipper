using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

[Table("PlexMovie")]
public class PlexMovie : PlexMedia
{
    public List<PlexRole> Roles { get; set; } = [];

    public List<PlexGenre> Genres { get; set; } = [];

    public List<PlexCountry> Country { get; set; } = [];

    [NotMapped]
    public List<PlexMediaDataPart> MovieParts => MovieData.SelectMany(x => x.Parts).ToList();

    [NotMapped]
    public List<PlexMediaData> MovieData => MediaData.MediaData;

    [NotMapped]
    public override PlexMediaType Type => PlexMediaType.Movie;
}
