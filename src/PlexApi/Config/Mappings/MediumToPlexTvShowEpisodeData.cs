using System;
using AutoMapper;
using PlexRipper.Domain;
using PlexRipper.PlexApi.Models;

namespace PlexRipper.PlexApi.Config.Mappings
{
    public class MediumToPlexTvShowEpisodeData : ITypeConverter<Medium, PlexTvShowEpisodeData>
    {
        public PlexTvShowEpisodeData Convert(Medium medium, PlexTvShowEpisodeData destination, ResolutionContext context)
        {
            try
            {
                var mapResult = context.Mapper.Map<PlexMediaData>(medium);
                return context.Mapper.Map<PlexTvShowEpisodeData>(mapResult);
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }
    }
}