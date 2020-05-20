using AutoMapper;
using PlexRipper.Domain.Entities;
using PlexRipper.Infrastructure.Common.DTO;
using PlexRipper.Infrastructure.Common.DTO.PlexGetServer;
using PlexRipper.Infrastructure.Common.DTO.PlexLibrary;
using System;
using System.Linq;

namespace PlexRipper.Infrastructure.Common.Mappings
{
    public class InfrastructureMappingProfile : Profile
    {
        public InfrastructureMappingProfile()
        {
            //PlexAccountDTO <-> PlexAccount
            CreateMap<PlexAccountDTO, PlexAccount>(MemberList.None)
                .ForMember(x => x.PlexAccountServers,
                    opt => opt.Ignore());
            CreateMap<PlexAccount, PlexAccountDTO>(MemberList.Destination)
                .ForMember(dto => dto.PlexServers,
                    opt => opt.MapFrom(x => x.PlexAccountServers.ToArray().Select(y => y.PlexServer).ToList()));


            CreateMap<PlexServerDTO, PlexServer>(MemberList.Destination)
                .ReverseMap();


            //PlexServerXML <-> PlexServer
            CreateMap<PlexServerXML, PlexServer>(MemberList.Destination)
                .ForMember(dest => dest.CreatedAt,
                    opt =>
                        opt.MapFrom(src => DateTimeOffset.FromUnixTimeSeconds(src.CreatedAt).UtcDateTime))
                .ForMember(dest => dest.UpdatedAt,
                    opt =>
                        opt.MapFrom(src => DateTimeOffset.FromUnixTimeSeconds(src.UpdatedAt).UtcDateTime))
            .ReverseMap();



            //PlexLibraryDirectoryDTO <-> PlexLibrary
            CreateMap<PlexLibraryDirectoryDTO, PlexLibrary>(MemberList.Source).ReverseMap();
        }
    }
}
