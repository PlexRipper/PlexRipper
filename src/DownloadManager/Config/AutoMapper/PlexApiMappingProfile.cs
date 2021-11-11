using AutoMapper;
using PlexRipper.Domain;

namespace PlexRipper.DownloadManager.AutoMapper
{
    public class DownloadManagerMappingProfile : Profile
    {
        public DownloadManagerMappingProfile()
        {
            // Metadata -> PlexMovie
            CreateMap<DownloadTask, DownloadProgressDTO>(MemberList.Destination)
                .ForMember(dto => dto.Status, entity => entity.MapFrom(x => x.DownloadStatus));
        }
    }
}