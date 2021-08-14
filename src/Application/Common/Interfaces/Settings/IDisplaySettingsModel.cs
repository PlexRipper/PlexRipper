using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface IDisplaySettingsModel
    {
        ViewMode TvShowViewMode { get; set; }

        ViewMode MovieViewMode { get; set; }
    }
}