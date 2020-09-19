using System;
using AutoMapper;
using PlexRipper.Domain;
using PlexRipper.PlexApi.Models;

namespace PlexRipper.PlexApi.Config.Mappings
{
    public class MediumToPlexMediaData : ITypeConverter<Medium, PlexMediaData>
    {
        public PlexMediaData Convert(Medium medium, PlexMediaData destination, ResolutionContext context)
        {
            try
            {
                if (medium == null)
                {
                    return null;
                }

                var plexMediaData = new PlexMediaData
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
                };

                // Sort any parts into the correct Part slot
                // for (int i = 0; i < medium.Part.Length; i++)
                // {
                //     var mapResult = context.Mapper.Map<PlexMediaDataPart>(medium.Part[i]);
                //     switch (i)
                //     {
                //         case 0:
                //             plexMediaData.Part1 = mapResult;
                //             break;
                //         case 1:
                //             plexMediaData.Part2 = mapResult;
                //             break;
                //         case 2:
                //             plexMediaData.Part3 = mapResult;
                //             break;
                //         case 3:
                //             plexMediaData.Part4 = mapResult;
                //             break;
                //         case 4:
                //             plexMediaData.Part5 = mapResult;
                //             break;
                //         case 5:
                //             plexMediaData.Part6 = mapResult;
                //             break;
                //         case 6:
                //             plexMediaData.Part7 = mapResult;
                //             break;
                //         case 7:
                //             plexMediaData.Part8 = mapResult;
                //             break;
                //     }
                // }

                return plexMediaData;
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }
    }
}