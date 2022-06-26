using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using PlexRipper.Domain;
using PlexRipper.Domain.AutoMapper.ValueConverters;
using PlexRipper.PlexApi.Models;
using Directory = PlexRipper.PlexApi.Models.Directory;

namespace PlexRipper.PlexApi.Mappings
{
    public class PlexApiMappingProfile : Profile
    {
        public PlexApiMappingProfile()
        {
            // PlexUser -> PlexAccount
            CreateMap<PlexAccountDTO, PlexAccount>(MemberList.None)
                .ForMember(dest => dest.PlexAccountServers, opt => opt.Ignore())
                .ForMember(dest => dest.PlexId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.AuthenticationToken, opt => opt.MapFrom(src => src.AuthToken))
                .ForMember(dest => dest.Uuid, opt => opt.MapFrom(src => src.Uuid))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Is2Fa, opt => opt.MapFrom(src => src.TwoFactorEnabled))
                .ForMember(dest => dest.HasPassword, opt => opt.MapFrom(src => src.HasPassword))
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // Server -> PlexServer
            CreateMap<Server, PlexServer>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PlexLibraries, opt => opt.Ignore())
                .ForMember(dest => dest.ServerStatus, opt => opt.Ignore())
                .ForMember(dest => dest.PlexAccountServers, opt => opt.Ignore())
                .ForMember(dest => dest.ServerFixApplyDNSFix, opt => opt.Ignore())
                .ForMember(dest => dest.DownloadTasks, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.ConvertUsing(new UnixLongStringToDateTimeUTC()))
                .ForMember(dest => dest.UpdatedAt,
                    opt => opt.ConvertUsing(new UnixLongStringToDateTimeUTC()));

            // MediaContainer -> PlexLibrary
            CreateMap<MediaContainer, PlexLibrary>(MemberList.None)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.ConvertUsing(new StringToPlexMediaTypeConverter()))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(x => x.Title1));

            // Directory -> PlexLibrary
            CreateMap<Directory, PlexLibrary>(MemberList.None)
                .ForMember(dest => dest.Type,
                    opt => opt.ConvertUsing(new StringToPlexMediaTypeConverter(), x => x.Type))
                .ForMember(dest => dest.LibraryLocationId,
                    opt => opt.MapFrom(src => src.Location.First().Id))

                // Location[0].Path -> LibraryLocationPath
                .ForMember(dest => dest.LibraryLocationPath,
                    opt => opt.MapFrom(src => src.Location.First().Path));

            // PlexMediaContainerDTO -> PlexMediaMetaData
            CreateMap<PlexMediaContainerDTO, PlexMediaMetaData>(MemberList.Destination)
                .ConvertUsing<PlexMediaMetaDataDTOPlexMediaMetaData>();

            // Metadata -> PlexMedia
            CreateMap<Metadata, PlexMedia>(MemberList.None)
                .ForMember(dest => dest.Key, opt => opt.MapFrom(x => x.RatingKey))
                .ForMember(dest => dest.MediaSize, opt => opt.MapFrom(x => x.Media.Sum(y => y.Part.Sum(z => z.Size))))
                .ForMember(dest => dest.SortTitle, opt => opt.MapFrom(x => x.TitleSort))
                .ForMember(dest => dest.HasThumb, opt => opt.MapFrom(x => !string.IsNullOrEmpty(x.Thumb)))
                .ForMember(dest => dest.HasBanner, opt => opt.MapFrom(x => !string.IsNullOrEmpty(x.Banner)))
                .ForMember(dest => dest.HasArt, opt => opt.MapFrom(x => !string.IsNullOrEmpty(x.Art)))
                .ForMember(dest => dest.HasTheme, opt => opt.MapFrom(x => !string.IsNullOrEmpty(x.Theme)))
                .ForMember(dest => dest.MetaDataKey, opt => opt.MapFrom(x => RetrieveMetaDataKey(x)))
                .ForPath(dest => dest.MediaData.MediaData, opt => opt.MapFrom(x => x.Media))
                .ForMember(dest => dest.OriginallyAvailableAt, opt => opt.ConvertUsing(new StringToDateTimeUtc()));

            // Medium -> PlexMediaData
            CreateMap<Medium, PlexMediaData>(MemberList.Destination)
                .ForMember(dest => dest.Parts, opt => opt.MapFrom(src => src.Part.ToList()))
                .ForMember(dest => dest.MediaFormat, opt => opt.MapFrom(src => src.Container));

            // Part -> PlexMediaDataPart
            CreateMap<Part, PlexMediaDataPart>(MemberList.Destination)
                .ForMember(dest => dest.ObfuscatedFilePath, opt => opt.MapFrom(x => x.Key));

            PlexMovieMappings();
            PlexTvShowMappings();
        }

        private void PlexMovieMappings()
        {
            // Metadata -> PlexMovie
            CreateMap<Metadata, PlexMovie>(MemberList.None)
                .IncludeBase<Metadata, PlexMedia>()
                .ForMember(dest => dest.FullTitle, opt => opt.MapFrom(x => $"{x.Title} ({x.Year})"))
                .ForMember(dest => dest.MovieData, opt => opt.MapFrom(x => x.Media));
        }

        private void PlexTvShowMappings()
        {
            // Metadata -> PlexTvShow
            CreateMap<Metadata, PlexTvShow>(MemberList.None)
                .IncludeBase<Metadata, PlexMedia>()
                .ForMember(dest => dest.FullTitle, opt => opt.MapFrom(x => x.Title));

            // Metadata -> PlexTvShowSeason
            CreateMap<Metadata, PlexTvShowSeason>(MemberList.None)
                .IncludeBase<Metadata, PlexMedia>()
                .ForMember(dest => dest.FullTitle, opt => opt.MapFrom(x =>  $"{x.ParentTitle}/{x.Title}" ))
                .ForMember(dest => dest.ParentKey, opt => opt.MapFrom(x => x.ParentRatingKey));

            // Metadata -> PlexTvShowEpisode
            CreateMap<Metadata, PlexTvShowEpisode>(MemberList.None)
                .IncludeBase<Metadata, PlexMedia>()
                .ForMember(dest => dest.FullTitle, opt => opt.MapFrom(x =>  $"{x.GrandparentTitle}/{x.ParentTitle}/{x.Title}" ))
                .ForMember(dest => dest.ParentKey, opt => opt.MapFrom(x => x.ParentRatingKey))
                .ForMember(dest => dest.EpisodeData, opt => opt.MapFrom(x => x.Media));
        }

        /// <summary>
        /// Retrieves the MetaDataKey from either the ThumbUrl,BannerUrl, ArtUrl or ThemeUrl.
        /// It is assumed that all MetaDataKeys are the same, returns 0 if nothing is found.
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        private int RetrieveMetaDataKey(Metadata metadata)
        {
            List<string> list = new()
            {
                metadata.Thumb,
                metadata.Banner,
                metadata.Art,
                metadata.Theme,
            };

            foreach (var entry in list)
            {
                if (!string.IsNullOrEmpty(entry))
                {
                    // We want the last number
                    // Example: /library/metadata/457047/thumb/1587006741
                    var splitStrings = entry.Split('/').ToList();
                    if (splitStrings.Count > 2)
                    {
                        if (int.TryParse(splitStrings.Last(), out int result))
                        {
                            return result;
                        }
                    }
                }
            }

            return 0;
        }
    }
}