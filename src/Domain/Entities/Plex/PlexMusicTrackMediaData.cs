namespace Reaparr.Domain;

public class PlexMusicTrackMediaData : BasePlexMediaData
{
    public override PlexMediaType Type => PlexMediaType.MusicTrack;

    public required int PlexTrackId { get; set; }

    public PlexMusicTrack? PlexTrack { get; set; }

    public ICollection<PlexMusicTrackDataStream> Streams { get; init; } = [];

    public required int PartIndex { get; set; }

    public string? OriginalFilePath { get; set; }

    public string? SourceRelativePath { get; set; }

    public int? AudioBitrateKbps { get; set; }

    public int? AudioBitDepth { get; set; }

    public int? AudioSampleRateHz { get; set; }

    public int? AudioChannels { get; set; }

    public string? AudioChannelLayout { get; set; }

    public string? AudioProfile { get; set; }

    public AudioQuality AudioQuality { get; set; } = AudioQuality.Unknown;
}
