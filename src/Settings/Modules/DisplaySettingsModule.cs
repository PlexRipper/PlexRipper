using PlexRipper.Settings.Models;
using Settings.Contracts;

namespace PlexRipper.Settings.Modules;

public class DisplaySettingsModule : BaseSettingsModule<IDisplaySettings>, IDisplaySettingsModule
{
    public ViewMode TvShowViewMode { get; set; }

    public ViewMode MovieViewMode { get; set; }

    public override string Name => "DisplaySettings";

    public override IDisplaySettings DefaultValues() =>
        new DisplaySettings { TvShowViewMode = ViewMode.Poster, MovieViewMode = ViewMode.Poster, };

    public override IDisplaySettings GetValues() =>
        new DisplaySettings { MovieViewMode = MovieViewMode, TvShowViewMode = TvShowViewMode, };
}
