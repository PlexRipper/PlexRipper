using AutoMapper;
using PlexRipper.Application.Common.DTO.Plex;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Entities.Plex;
using PlexRipper.Domain.ValueObjects;
using System;

namespace PlexRipper.Application.Common.Mappings
{
    public class ApplicationMappingProfile : Profile
    {
        public ApplicationMappingProfile()
        {
            CreateMap<PlexAccountDTO, PlexAccount>(MemberList.Destination).ReverseMap();
            CreateMap<AccountDTO, Account>(MemberList.Destination).ReverseMap();
            CreateMap<ServerInfoDTO, PlexServer>(MemberList.Destination)
                .ForMember(x => x.CreatedAt,
                    opt =>
                        opt.MapFrom((s, d) =>
                            DateTimeOffset.FromUnixTimeSeconds(s.CreatedAt).DateTime))
                .ForMember(x => x.UpdatedAt,
                    opt =>
                        opt.MapFrom((s, d) =>
                            DateTimeOffset.FromUnixTimeSeconds(s.UpdatedAt).DateTime))
                .ReverseMap();
        }
    }
}
