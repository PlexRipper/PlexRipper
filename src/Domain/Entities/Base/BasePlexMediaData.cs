using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class BasePlexMediaData : PlexMedia
{
    public required MediaDataContainer MediaData { get; init; }

    [NotMapped]
    public List<LibraryMediaItemMediaDTO> MetaDataList => MediaData.MediaData;

    [NotMapped]
    public List<PlexMediaQuality> Qualities
    {
        get
        {
            return MetaDataList
                .Select(y => new PlexMediaQuality(y.VideoResolution))
                .Reverse() // This sorts from lowest to highest quality
                .TakeLast(1) // TODO:remove this when quality selector for downloading is implemented
                .ToList();
        }
    }
}
