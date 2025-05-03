using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class BasePlexMediaData : PlexMedia
{
    public ICollection<BasePlexMediaDataRow> MediaDataList { get; set; }

    [NotMapped]
    public List<PlexMediaQuality> Qualities
    {
        get
        {
            return MediaDataList
                .Select(y => new PlexMediaQuality(y.VideoResolution))
                .Reverse() // This sorts from lowest to highest quality
                .TakeLast(1) // TODO:remove this when quality selector for downloading is implemented
                .ToList();
        }
    }
}
