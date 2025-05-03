using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

[Table("PlexMovie")]
public class PlexMovie : BasePlexMediaData
{
    public required List<PlexRole> Roles { get; set; } = [];

    public required List<PlexGenre> Genres { get; set; } = [];

    public required List<PlexCountry> Countries { get; set; } = [];

    public new ICollection<PlexMovieMediaData> MediaDataList { get; set; } = [];

    [NotMapped]
    public override PlexMediaType Type => PlexMediaType.Movie;
}
