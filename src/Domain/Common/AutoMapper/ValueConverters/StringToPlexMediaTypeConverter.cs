using AutoMapper;

namespace PlexRipper.Domain.AutoMapper.ValueConverters
{
    public class StringToPlexMediaTypeConverter : IValueConverter<string, PlexMediaType>
    {
        public PlexMediaType Convert(string sourceMember, ResolutionContext context)
        {
            return sourceMember.ToLower() switch
            {
                "movie" => PlexMediaType.Movie,
                "show" => PlexMediaType.TvShow,
                "artist" => PlexMediaType.Music,
                "season" => PlexMediaType.Season,
                "episode" => PlexMediaType.Episode,
                "music" => PlexMediaType.Music,
                "album" => PlexMediaType.Album,
                _ => PlexMediaType.Unknown,
            };
        }
    }
}