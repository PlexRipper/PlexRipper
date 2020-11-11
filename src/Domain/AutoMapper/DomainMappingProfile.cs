using System;
using AutoMapper;
using PlexRipper.Domain.AutoMapper.ValueConverters;

namespace PlexRipper.Domain.AutoMapper
{
    /// <summary>
    /// Contains all generic primitive mappings and entity mappings
    /// </summary>
    public class DomainMappingProfile : Profile
    {
        public DomainMappingProfile()
        {
            // String -> Guid
            CreateMap<string, Guid>().ConvertUsing(s => Guid.Parse(s));
            CreateMap<Guid, string>().ConvertUsing(g => g.ToString("N"));
            CreateMap<long, DateTime>().ConvertUsing(g => DateTimeOffset.FromUnixTimeSeconds(g).DateTime.ToUniversalTime());
            CreateMap<DateTime, long>().ConvertUsing(g => new DateTimeOffset(g).ToUnixTimeSeconds());

            // Entities
            // PlexAccountServer => PlexServer
            CreateMap<PlexAccountServer, PlexServer>(MemberList.Destination)
                .ConvertUsing(source => source.PlexServer ?? null);

            // PlexMediaData -> PlexMovieData
            CreateMap<PlexMediaData, PlexMovieData>(MemberList.Source).ReverseMap();

            // PlexMediaDataPart -> PlexMovieDataPart
            CreateMap<PlexMediaDataPart, PlexMovieDataPart>(MemberList.Source).ReverseMap();
        }
    }
}