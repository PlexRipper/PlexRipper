using System;
using AutoMapper;
using PlexRipper.Domain;
using PlexRipper.PlexApi.Models;

namespace PlexRipper.PlexApi.Config.Mappings
{
    public class MediumToPlexMovieData : ITypeConverter<Medium, PlexMovieData>
    {
        public PlexMovieData Convert(Medium medium, PlexMovieData destination, ResolutionContext context)
        {
            try
            {
                var mapResult = context.Mapper.Map<PlexMediaData>(medium);
                return context.Mapper.Map<PlexMovieData>(mapResult);
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }
    }
}