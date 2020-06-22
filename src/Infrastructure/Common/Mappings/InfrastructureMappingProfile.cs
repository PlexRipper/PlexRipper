using AutoMapper;
using PlexRipper.Domain.AutoMapper.ValueConverters;
using PlexRipper.Domain.Entities;
using PlexRipper.Infrastructure.Common.DTO;
using PlexRipper.Infrastructure.Common.DTO.PlexGetLibrarySections;
using PlexRipper.Infrastructure.Common.DTO.PlexGetServer;
using PlexRipper.Infrastructure.Common.DTO.PlexGetStatus;
using PlexRipper.Infrastructure.Common.DTO.PlexLibrary;
using PlexRipper.Infrastructure.Common.DTO.PlexLibraryMedia;
using System.Linq;

namespace PlexRipper.Infrastructure.Common.Mappings
{
    public class InfrastructureMappingProfile : Profile
    {
        public InfrastructureMappingProfile()
        {
            //PlexAccountDTO -> PlexAccount
            CreateMap<PlexAccountDTO, PlexAccount>(MemberList.None)
                .ForMember(dest => dest.PlexAccountServers, opt => opt.Ignore())
                .ForMember(dest => dest.PlexId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Id, opt => opt.Ignore());


            //PlexServerDTO -> PlexServer
            CreateMap<PlexServerDTO, PlexServer>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PlexLibraries, opt => opt.Ignore())
                .ReverseMap();


            //PlexServerXML -> PlexServer
            CreateMap<PlexServerXML, PlexServer>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PlexLibraries, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.ConvertUsing(new UnixLongStringToDateTimeUTC()))
                .ForMember(dest => dest.UpdatedAt,
                    opt => opt.ConvertUsing(new UnixLongStringToDateTimeUTC()))
            .ReverseMap();

            // PlexLibraryDirectoryDTO <-> PlexLibrary
            // TODO Need to check which correct DTO is returned by the PlexApi before checking here
            CreateMap<PlexLibraryContainerDTO, PlexLibrary>(MemberList.None);


            // PlexLibrarySectionsDirectoryDTO -> PlexLibrary
            CreateMap<PlexLibrarySectionsDirectoryDTO, PlexLibrary>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PlexServerId, opt => opt.Ignore())
                .ForMember(dest => dest.PlexServer, opt => opt.Ignore())
                .ForMember(dest => dest.Movies, opt => opt.Ignore())
                .ForMember(dest => dest.Series, opt => opt.Ignore())
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

            // PlexLibraryMetaDataDTO -> PlexSerie
            CreateMap<PlexLibraryMetaDataDTO, PlexSerie>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PlexSerieGenres, opt => opt.Ignore())
                .ForMember(dest => dest.PlexSerieRoles, opt => opt.Ignore())
                .ForMember(dest => dest.PlexLibrary, opt => opt.Ignore())
                .ForMember(dest => dest.PlexLibraryId, opt => opt.Ignore())
                .ForMember(dest => dest.LastViewedAt, opt => opt.Ignore())
                .ForMember(dest => dest.OriginallyAvailableAt,
                    opt => opt.ConvertUsing(new StringToDateTimeUTC()));

            CreateMap<PlexMediaMetaDataDTO, PlexMediaMetaData>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => GetMedia(src).Duration))
                .ForMember(dest => dest.Bitrate, opt => opt.MapFrom(src => GetMedia(src).Bitrate))
                .ForMember(dest => dest.Width, opt => opt.MapFrom(src => GetMedia(src).Width))
                .ForMember(dest => dest.Height, opt => opt.MapFrom(src => GetMedia(src).Height))
                .ForMember(dest => dest.AspectRatio, opt => opt.MapFrom(src => GetMedia(src).AspectRatio))
                .ForMember(dest => dest.AudioChannels, opt => opt.MapFrom(src => GetMedia(src).AudioChannels))
                .ForMember(dest => dest.AudioCodec, opt => opt.MapFrom(src => GetMedia(src).AudioCodec))
                .ForMember(dest => dest.VideoCodec, opt => opt.MapFrom(src => GetMedia(src).VideoCodec))
                .ForMember(dest => dest.VideoResolution, opt => opt.MapFrom(src => GetMedia(src).VideoResolution))
                .ForMember(dest => dest.MediaFormat, opt => opt.MapFrom(src => GetMedia(src).Container))
                .ForMember(dest => dest.VideoFrameRate, opt => opt.MapFrom(src => GetMedia(src).VideoFrameRate))
                .ForMember(dest => dest.AudioProfile, opt => opt.MapFrom(src => GetMedia(src).AudioProfile))
                .ForMember(dest => dest.VideoProfile, opt => opt.MapFrom(src => GetMedia(src).VideoProfile))
                .ForMember(dest => dest.FilePath, opt => opt.MapFrom(src => GetMedia(src).Part.First().File))
                .ForMember(dest => dest.ObfuscatedFilePath, opt => opt.MapFrom(src => GetMedia(src).Part.First().Key));
        }

        private Medium GetMedia(PlexMediaMetaDataDTO src)
        {
            if (!src.MediaContainerDto.Metadata.Any() && !src.MediaContainerDto.Metadata.First().Media.Any())
            {
                return null;
            }
            return GetMedia(src);
        }
    }
}
