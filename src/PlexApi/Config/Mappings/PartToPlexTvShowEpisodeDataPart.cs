using System;
using AutoMapper;
using PlexRipper.Domain;
using PlexRipper.PlexApi.Models;

namespace PlexRipper.PlexApi.Config.Mappings
{
    public class PartToPlexTvShowEpisodeDataPart : ITypeConverter<Part, PlexTvShowEpisodeDataPart>
    {
        public PlexTvShowEpisodeDataPart Convert(Part part, PlexTvShowEpisodeDataPart destination, ResolutionContext context)
        {
            try
            {
                var mapResult = context.Mapper.Map<PlexMediaDataPart>(part);
                return context.Mapper.Map<PlexTvShowEpisodeDataPart>(mapResult);
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }
    }
}