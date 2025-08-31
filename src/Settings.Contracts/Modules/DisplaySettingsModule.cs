using Reaparr.Domain;

namespace Reaparr.Settings.Contracts;

public record DisplaySettingsModule : BaseSettingsModule<DisplaySettingsModule>, IDisplaySettings
{
    private ViewMode _tvShowViewMode = ViewMode.Poster;
    private ViewMode _movieViewMode = ViewMode.Poster;
    private PlexMediaType _allOverviewViewMode = PlexMediaType.Movie;

    public static DisplaySettingsModule Create() =>
        new()
        {
            TvShowViewMode = ViewMode.Poster,
            MovieViewMode = ViewMode.Poster,
            AllOverviewViewMode = PlexMediaType.Movie,
        };

    public required ViewMode TvShowViewMode
    {
        get => _tvShowViewMode;
        set => SetProperty(ref _tvShowViewMode, value);
    }

    public required ViewMode MovieViewMode
    {
        get => _movieViewMode;
        set => SetProperty(ref _movieViewMode, value);
    }

    public required PlexMediaType AllOverviewViewMode
    {
        get => _allOverviewViewMode;
        set => SetProperty(ref _allOverviewViewMode, value);
    }
}
