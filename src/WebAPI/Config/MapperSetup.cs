using AutoMapper;
using PlexRipper.Application;
using PlexRipper.Domain.AutoMapper;
using PlexRipper.DownloadManager.AutoMapper;
using PlexRipper.PlexApi.Config.Mappings;

namespace PlexRipper.WebAPI.Config
{
    public static class MapperSetup
    {
        public static MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new DomainMappingProfile());
            cfg.AddProfile(new ApplicationMappingProfile());
            cfg.AddProfile(new PlexApiMappingProfile());
            cfg.AddProfile(new DownloadManagerMappingProfile());
            cfg.AddProfile(new WebApiMappingProfile());
        });

        public static IMapper CreateMapper()
        {
            return Configuration.CreateMapper();
        }
    }
}