using PlexRipper.Domain;

namespace Settings.Contracts;

public class DisplaySettingsDTO : IDisplaySettings
{
    public ViewMode TvShowViewMode { get; set; }

    public ViewMode MovieViewMode { get; set; }
}