using AutoMapper;
using System;

namespace PlexRipper.Domain.AutoMapper
{
    /// <summary>
    /// Contains all generic primitive mappings and entity mappings
    /// </summary>
    public class DomainMappingProfile : Profile
    {
        public DomainMappingProfile()
        {
            //String -> Guid
            CreateMap<string, Guid>().ConvertUsing(s => Guid.Parse(s));
            CreateMap<Guid, string>().ConvertUsing(g => g.ToString("N"));
        }
    }
}
