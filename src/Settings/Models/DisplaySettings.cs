using PlexRipper.Application;

namespace PlexRipper.Settings.Models;

public class DisplaySettings : IDisplaySettings
{
    public ViewMode TvShowViewMode { get; set; }

    public ViewMode MovieViewMode { get; set; }
}