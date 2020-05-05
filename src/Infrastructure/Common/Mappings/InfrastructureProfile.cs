using AutoMapper;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Entities.Plex;
using PlexRipper.Domain.ValueObjects;
using PlexRipper.Infrastructure.Common.Models.Plex;
using System;

namespace PlexRipper.Infrastructure.Common.Mappings
{
    public class InfrastructureProfile : Profile
    {
        public InfrastructureProfile()
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
