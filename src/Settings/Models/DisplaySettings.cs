using Settings.Contracts;

namespace PlexRipper.Settings;

public record DisplaySettings : IDisplaySettings
{
    public ViewMode TvShowViewMode { get; set; } = ViewMode.Poster;

    public ViewMode MovieViewMode { get; set; } = ViewMode.Poster;
}
