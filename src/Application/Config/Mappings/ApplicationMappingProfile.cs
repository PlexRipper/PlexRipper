using AutoMapper;
using PlexRipper.Application.Common.DTO.WebApi;
using PlexRipper.Domain;

namespace PlexRipper.Application.Config.Mappings
{
    public class ApplicationMappingProfile : Profile
    {
        public ApplicationMappingProfile()
        {
            // DownloadTask -> DownloadTaskDTO
            CreateMap<DownloadTask, DownloadTaskDTO>(MemberList.Destination)
                .ForMember(dto => dto.Children, opt => opt.Ignore())
                .ForMember(dto => dto.FullTitle, opt => opt.Ignore())
                .ForMember(dto => dto.Actions, opt => opt.Ignore())
                .ForMember(dto => dto.TimeRemaining, opt => opt.Ignore())
                .ForMember(dto => dto.WorkerProgresses, opt => opt.Ignore())
                .ForMember(dto => dto.Status, entity => entity.MapFrom(x => x.DownloadStatus));
        }
    }
}