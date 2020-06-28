using AutoMapper;
using PlexRipper.Domain.Entities;
using PlexRipper.PlexApi.Common.DTO.PlexGetStatus;
using System.Linq;

namespace PlexRipper.PlexApi.Config.Mappings
{
    public class PlexMediaMetaDataDTOPlexMediaMetaData : ITypeConverter<PlexMediaMetaDataDTO, PlexMediaMetaData>
    {
        public PlexMediaMetaData Convert(PlexMediaMetaDataDTO source, PlexMediaMetaData destination, ResolutionContext context)
        {
            if (source?.MediaContainerDto == null || !source.MediaContainerDto.Metadata.Any() || !source.MediaContainerDto.Metadata.First().Media.Any())
            {
                return null;
            }

            var medium = source.MediaContainerDto.Metadata.First().Media.First();
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
                ObfuscatedFilePath = part != null ? part.Key : "",
            };
        }
    }
}
