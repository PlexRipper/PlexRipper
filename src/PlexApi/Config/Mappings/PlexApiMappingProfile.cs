using AutoMapper;
using PlexRipper.Domain.AutoMapper.ValueConverters;
using PlexRipper.PlexApi.Api;
using PlexRipper.PlexApi.Api.Users.SignIn;
using PlexRipper.PlexApi.Models;

namespace PlexRipper.PlexApi.Mappings;

public class PlexApiMappingProfile : Profile
{
    public PlexApiMappingProfile()
    {
        // SignInResponse -> PlexAccount
        CreateMap<SignInResponse, PlexAccount>(MemberList.None)
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

        // PlexMediaContainerDTO -> PlexMediaMetaData
        CreateMap<PlexMediaContainerDTO, PlexMediaMetaData>(MemberList.None)
            .ConstructUsing((source, context) =>
            {
                if (source?.MediaContainer == null || !source.MediaContainer.Metadata.Any() || !source.MediaContainer.Metadata.First().Media.Any())
                    return null;

                var metaData = source?.MediaContainer?.Metadata?.First();
                var medium = metaData.Media.First();
                var part = medium.Part.Any() ? medium.Part.First() : null;

                return new PlexMediaMetaData
                {
                    Duration = medium.Duration,
                    Bitrate = medium.Bitrate,
                    Width = medium.Width,
                    Height = medium.Height,
                    AspectRatio = medium.AspectRatio,
                    AudioChannels = medium.AudioChannels,
                    AudioCodec = medium.AudioCodec,
                    VideoCodec = medium.VideoCodec,
                    VideoResolution = medium.VideoResolution,
                    MediaFormat = medium.Container,
                    VideoFrameRate = medium.VideoFrameRate,
                    AudioProfile = medium.AudioProfile,
                    VideoProfile = medium.VideoProfile,
                    FilePath = part != null ? part.File : "",
                    Title = metaData.Title,
                    ObfuscatedFilePath = part != null ? part.Key : "",
                    TitleTvShow = metaData.GrandparentTitle,
                    TitleTvShowSeason = metaData.ParentTitle,
                    RatingKey = int.TryParse(metaData.RatingKey, out var result) ? result : 0,
                };
            });

        PlexServerMappings();
        PlexLibraryMappings();
    }

    private void PlexServerMappings()
    {
        // Server -> PlexServer
        CreateMap<ServerResource, PlexServer>(MemberList.Destination)
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Name, opt => opt.MapFrom(x => x.Name))
            .ForMember(dest => dest.Product, opt => opt.MapFrom(x => x.Product))
            .ForMember(dest => dest.ProductVersion, opt => opt.MapFrom(x => x.ProductVersion))
            .ForMember(dest => dest.Platform, opt => opt.MapFrom(x => x.Platform))
            .ForMember(dest => dest.PlatformVersion, opt => opt.MapFrom(x => x.PlatformVersion))
            .ForMember(dest => dest.Device, opt => opt.MapFrom(x => x.Device))
            .ForMember(dest => dest.MachineIdentifier, opt => opt.MapFrom(x => x.ClientIdentifier))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(x => x.CreatedAt))
            .ForMember(dest => dest.LastSeenAt, opt => opt.MapFrom(x => x.LastSeenAt))
            .ForMember(dest => dest.Provides, opt => opt.MapFrom(x => x.Provides))
            .ForMember(dest => dest.OwnerId, opt => opt.MapFrom(x => x.OwnerId))
            .ForMember(dest => dest.PlexServerOwnerUsername, opt => opt.MapFrom(x => x.SourceTitle))
            .ForMember(dest => dest.PublicAddress, opt => opt.MapFrom(x => x.PublicAddress))

            // Server flags
            .ForMember(dest => dest.Owned, opt => opt.MapFrom(x => x.Owned))
            .ForMember(dest => dest.Home, opt => opt.MapFrom(x => x.Home))
            .ForMember(dest => dest.Synced, opt => opt.MapFrom(x => x.Synced))
            .ForMember(dest => dest.Relay, opt => opt.MapFrom(x => x.Relay))
            .ForMember(dest => dest.Presence, opt => opt.MapFrom(x => x.Presence))
            .ForMember(dest => dest.HttpsRequired, opt => opt.MapFrom(x => x.HttpsRequired))
            .ForMember(dest => dest.PublicAddressMatches, opt => opt.MapFrom(x => x.PublicAddressMatches))
            .ForMember(dest => dest.DnsRebindingProtection, opt => opt.MapFrom(x => x.DnsRebindingProtection))
            .ForMember(dest => dest.NatLoopbackSupported, opt => opt.MapFrom(x => x.NatLoopbackSupported))

            // relations
            .ForMember(dest => dest.PlexLibraries, opt => opt.Ignore())
            .ForMember(dest => dest.ServerStatus, opt => opt.Ignore())
            .ForMember(dest => dest.PlexAccountServers, opt => opt.Ignore())
            .ForMember(dest => dest.ServerFixApplyDNSFix, opt => opt.Ignore())
            .ForMember(dest => dest.DownloadTasks, opt => opt.Ignore())
            .ForMember(dest => dest.PreferredConnectionId, opt => opt.Ignore())
            .ForMember(dest => dest.PlexServerConnections, opt => opt.MapFrom(x => x.Connections));

        CreateMap<ServerResourceConnection, PlexServerConnection>(MemberList.Destination)
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PlexServer, opt => opt.Ignore())
            .ForMember(dest => dest.PlexServerId, opt => opt.Ignore())
            .ForMember(dest => dest.IPv4, opt => opt.MapFrom(x => x.Address.IsIpAddress() && !x.IPv6))

            // The port fix is when we don't want to use the port when Address is a domain name
            .ForMember(dest => dest.PortFix, opt => opt.MapFrom(x => !x.Address.IsIpAddress() && !x.IPv6 && x.Address != "localhost"))
            .ForMember(dest => dest.PlexServerStatus, opt => opt.Ignore());

        CreateMap<ServerResource, ServerAccessTokenDTO>(MemberList.Destination)
            .ForMember(dest => dest.PlexAccountId, opt => opt.Ignore())
            .ForMember(dest => dest.AccessToken, opt => opt.MapFrom(x => x.AccessToken))
            .ForMember(dest => dest.MachineIdentifier, opt => opt.MapFrom(x => x.ClientIdentifier));
    }

    private void PlexLibraryMappings()
    {
        // MediaContainer -> PlexLibrary
        CreateMap<MediaContainer, PlexLibrary>(MemberList.None)
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.ConvertUsing(new StringToPlexMediaTypeConverter()))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(x => x.Title1));

        // LibrariesResponseDirectory -> PlexLibrary
        CreateMap<LibrariesResponseDirectory, PlexLibrary>(MemberList.None)
            .ForMember(dest => dest.Type,
                opt => opt.ConvertUsing(new StringToPlexMediaTypeConverter(), x => x.Type))
            .ForMember(dest => dest.LibraryLocationId,
                opt => opt.MapFrom(src => src.Location.First().Id))

            // Location[0].Path -> LibraryLocationPath
            .ForMember(dest => dest.LibraryLocationPath,
                opt => opt.MapFrom(src => src.Location.First().Path));
    }
}