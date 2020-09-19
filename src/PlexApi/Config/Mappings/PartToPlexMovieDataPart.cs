using System;
using AutoMapper;
using PlexRipper.Domain;
using PlexRipper.PlexApi.Models;

namespace PlexRipper.PlexApi.Config.Mappings
{
    public class PartToPlexMovieDataPart : ITypeConverter<Part, PlexMovieDataPart>
    {
        public PlexMovieDataPart Convert(Part part, PlexMovieDataPart destination, ResolutionContext context)
        {
            try
            {
                var mapResult = context.Mapper.Map<PlexMediaDataPart>(part);
                return context.Mapper.Map<PlexMovieDataPart>(mapResult);
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }
    }
}