using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

[Table("PlexMovie")]
public class PlexMovie : PlexMedia
{
    public List<PlexRole> Roles { get; set; } = [];

    public List<PlexGenre> Genres { get; set; } = [];

    public List<PlexCountry> Countries { get; set; } = [];

    [NotMapped]
    public override PlexMediaType Type => PlexMediaType.Movie;
}
