using Riok.Mapperly.Abstractions;

namespace PlexRipper.PlexApi;

[Mapper(UseDeepCloning = true)]
public static partial class PlexMediaMapper
{
    public static partial PlexMovie ToPlexMovie(this PlexMedia source);

    public static partial PlexTvShow ToPlexTvShow(this PlexMedia source);

    public static partial PlexTvShowSeason ToPlexTvShowSeason(this PlexMedia source);

    public static partial PlexTvShowEpisode ToPlexTvShowEpisode(this PlexMedia source);
}