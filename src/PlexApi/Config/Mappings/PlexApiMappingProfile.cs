using AutoMapper;
using PlexRipper.Domain.AutoMapper.ValueConverters;
using PlexRipper.Domain.Entities;
using PlexRipper.PlexApi.Common.DTO;
using PlexRipper.PlexApi.Common.DTO.PlexGetLibrarySections;
using PlexRipper.PlexApi.Common.DTO.PlexGetServer;
using PlexRipper.PlexApi.Common.DTO.PlexGetStatus;
using PlexRipper.PlexApi.Common.DTO.PlexLibrary;
using PlexRipper.PlexApi.Common.DTO.PlexLibraryMedia;
using System.Linq;

namespace PlexRipper.PlexApi.Config.Mappings
{
    public class PlexApiMappingProfile : Profile
    {
        public PlexApiMappingProfile()
        {
            //PlexAccountDTO -> PlexAccount
            CreateMap<PlexAccountDTO, PlexAccount>(MemberList.None)
                .ForMember(dest => dest.PlexAccountServers, opt => opt.Ignore())
                .ForMember(dest => dest.PlexId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Id, opt => opt.Ignore());


            //PlexServerDTO <-> PlexServer
            CreateMap<PlexServerDTO, PlexServer>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PlexLibraries, opt => opt.Ignore())
                .ForMember(dest => dest.ServerStatus, opt => opt.Ignore())
                .ReverseMap();

            //PlexLibraryMediaDTO -> PlexLibrary
            CreateMap<PlexLibraryMediaDTO, PlexLibrary>(MemberList.None)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.MapFrom(x => x.MediaContainer.ViewGroup))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(x => x.MediaContainer.Title1));

            //PlexServerXML -> PlexServer
            CreateMap<PlexServerXML, PlexServer>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PlexLibraries, opt => opt.Ignore())
                .ForMember(dest => dest.ServerStatus, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.ConvertUsing(new UnixLongStringToDateTimeUTC()))
                .ForMember(dest => dest.UpdatedAt,
                    opt => opt.ConvertUsing(new UnixLongStringToDateTimeUTC()))
            .ReverseMap();

            // PlexLibraryDirectoryDTO <-> PlexLibrary
            // TODO Need to check which correct DTO is returned by the PlexApi before checking here
            CreateMap<PlexLibraryContainerDTO, PlexLibrary>(MemberList.None);

            //PlexAccountDTO -> PlexAccount
            CreateMap<PlexStatusDTO, PlexServerStatus>(MemberList.None)
                .ForMember(dest => dest.LastChecked, opt => opt.Ignore());

            // PlexLibrarySectionsDirectoryDTO -> PlexLibrary
            CreateMap<PlexLibrarySectionsDirectoryDTO, PlexLibrary>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PlexServerId, opt => opt.Ignore())
                .ForMember(dest => dest.PlexServer, opt => opt.Ignore())
                .ForMember(dest => dest.Movies, opt => opt.Ignore())
                .ForMember(dest => dest.TvShows, opt => opt.Ignore())
                // Location[0].Id -> LibraryLocationId
                .ForMember(dest => dest.LibraryLocationId,
                    opt => opt.MapFrom(src => src.Location.First().Id))
                // Location[0].Path -> LibraryLocationPath
                .ForMember(dest => dest.LibraryLocationPath,
                    opt => opt.MapFrom(src => src.Location.First().Path));

            // PlexLibraryMetaDataDTO -> PlexMovies
            CreateMap<PlexLibraryMetaDataDTO, PlexMovie>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PlexMovieGenres, opt => opt.Ignore())
                .ForMember(dest => dest.PlexMovieRoles, opt => opt.Ignore())
                .ForMember(dest => dest.PlexLibrary, opt => opt.Ignore())
                .ForMember(dest => dest.PlexLibraryId, opt => opt.Ignore())
                .ForMember(dest => dest.LastViewedAt, opt => opt.Ignore())
                .ForMember(dest => dest.OriginallyAvailableAt,
                    opt => opt.ConvertUsing(new StringToDateTimeUTC()));

            // PlexLibraryMetaDataDTO -> PlexTvShow
            CreateMap<PlexLibraryMetaDataDTO, PlexTvShow>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PlexTvShowGenres, opt => opt.Ignore())
                .ForMember(dest => dest.PlexTvShowRoles, opt => opt.Ignore())
                .ForMember(dest => dest.PlexLibrary, opt => opt.Ignore())
                .ForMember(dest => dest.PlexLibraryId, opt => opt.Ignore())
                .ForMember(dest => dest.LastViewedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Seasons, opt => opt.Ignore())
                .ForMember(dest => dest.OriginallyAvailableAt,
                    opt => opt.ConvertUsing(new StringToDateTimeUTC()));

            // PlexMediaMetaDataDTO -> PlexMediaMetaData
            CreateMap<PlexMediaMetaDataDTO, PlexMediaMetaData>(MemberList.Destination)
                .ConvertUsing<PlexMediaMetaDataDTOPlexMediaMetaData>();

        }
    }
}
