using PlexRipper.Domain;

namespace Settings.Contracts;

public record DisplaySettingsModule : BaseSettingsModule<DisplaySettingsModule>, IDisplaySettings
{
    private ViewMode _tvShowViewMode = ViewMode.Poster;
    private ViewMode _movieViewMode = ViewMode.Poster;

    public ViewMode TvShowViewMode
    {
        get => _tvShowViewMode;
        set => SetProperty(ref _tvShowViewMode, value);
    }

    public ViewMode MovieViewMode
    {
        get => _movieViewMode;
        set => SetProperty(ref _movieViewMode, value);
    }
}
