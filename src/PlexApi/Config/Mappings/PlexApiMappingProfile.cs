using AutoMapper;
using PlexRipper.Domain.AutoMapper.ValueConverters;
using PlexRipper.Domain.Entities;
using PlexRipper.PlexApi.Models;
using PlexRipper.PlexApi.Models.Server;
using PlexAccount = PlexRipper.Domain.Entities.PlexAccount;

namespace PlexRipper.PlexApi.Config.Mappings
{
    public class PlexApiMappingProfile : Profile
    {
        public PlexApiMappingProfile()
        {
            //PlexUser -> PlexAccount
            CreateMap<PlexUser, PlexAccount>(MemberList.None)
                .ForMember(dest => dest.PlexAccountServers, opt => opt.Ignore())
                .ForMember(dest => dest.PlexId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Id, opt => opt.Ignore());


            //Server <-> PlexServer
            CreateMap<Server, PlexServer>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PlexLibraries, opt => opt.Ignore())
                .ForMember(dest => dest.ServerStatus, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.ConvertUsing(new UnixLongStringToDateTimeUTC()))
                .ForMember(dest => dest.UpdatedAt,
                    opt => opt.ConvertUsing(new UnixLongStringToDateTimeUTC()))
                .ReverseMap();

            //MediaContainer -> PlexLibrary
            CreateMap<MediaContainer, PlexLibrary>(MemberList.None)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.MapFrom(x => x.ViewGroup))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(x => x.Title1));

            // MediaContainer -> PlexMovie
            CreateMap<Metadata, PlexMovie>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PlexMovieGenres, opt => opt.Ignore())
                .ForMember(dest => dest.PlexMovieRoles, opt => opt.Ignore())
                .ForMember(dest => dest.PlexLibrary, opt => opt.Ignore())
                .ForMember(dest => dest.PlexLibraryId, opt => opt.Ignore())
                .ForMember(dest => dest.LastViewedAt, opt => opt.Ignore())
                .ForMember(dest => dest.RatingKey, opt => opt.Ignore())
                .ForMember(dest => dest.OriginallyAvailableAt,
                    opt => opt.ConvertUsing(new StringToDateTimeUTC()));

            // MediaContainer -> PlexTvShow
            CreateMap<Metadata, PlexTvShow>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PlexTvShowGenres, opt => opt.Ignore())
                .ForMember(dest => dest.PlexTvShowRoles, opt => opt.Ignore())
                .ForMember(dest => dest.PlexLibrary, opt => opt.Ignore())
                .ForMember(dest => dest.PlexLibraryId, opt => opt.Ignore())
                .ForMember(dest => dest.LastViewedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Seasons, opt => opt.Ignore())
                .ForMember(dest => dest.OriginallyAvailableAt,
                    opt => opt.ConvertUsing(new StringToDateTimeUTC()));

            // PlexMediaContainer -> PlexMediaMetaData
            CreateMap<PlexMediaContainer, PlexMediaMetaData>(MemberList.Destination)
                .ConvertUsing<PlexMediaMetaDataDTOPlexMediaMetaData>();

        }
    }
}
