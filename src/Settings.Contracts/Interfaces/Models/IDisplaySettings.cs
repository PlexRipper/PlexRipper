using PlexRipper.Domain;

namespace Settings.Contracts;

public interface IDisplaySettings
{
    ViewMode TvShowViewMode { get; set; }

    ViewMode MovieViewMode { get; set; }
}