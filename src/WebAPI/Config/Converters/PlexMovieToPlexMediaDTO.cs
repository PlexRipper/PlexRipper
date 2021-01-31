using AutoMapper;
using PlexRipper.Domain;
using PlexRipper.WebAPI.Common.DTO;

namespace PlexRipper.WebAPI.Config.Converters
{
    public class PlexMovieToPlexMediaDTO : ITypeConverter<PlexMovie, PlexMediaDTO>
    {
        public PlexMediaDTO Convert(PlexMovie source, PlexMediaDTO destination, ResolutionContext context)
        {
            return new PlexMediaDTO();
        }
    }
}