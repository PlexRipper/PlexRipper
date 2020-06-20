using AutoMapper;
using Carter;
using Carter.Request;
using Carter.Response;
using Microsoft.AspNetCore.Http;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.WebAPI.Common.DTO;
using System.Threading.Tasks;

namespace PlexRipper.WebAPI.Controllers
{
    public class PlexLibraryController : CarterModule
    {
        private readonly IPlexService _plexService;
        private readonly IPlexLibraryService _plexLibraryService;
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;

        public PlexLibraryController(IPlexService plexService, IPlexLibraryService plexLibraryService, IAccountService accountService, IMapper mapper) : base("/api")
        {
            _plexService = plexService;
            _plexLibraryService = plexLibraryService;
            _accountService = accountService;
            _mapper = mapper;
            string path = "/plex/library";

            Get(path + "/{id:int}", GetLibraryAsync); //

        }

        private Task GetLibraryAsync(HttpRequest req, HttpResponse res)
        {
            int libraryId = req.RouteValues.As<int>("id");
            bool refresh = req.Query["refresh"].ToString() == "1";
            var data = _plexLibraryService.GetPlexLibraryAsync(libraryId, refresh);
            var result = _mapper.Map<PlexLibraryContainerDTO>(data);
            return res.AsJson(result);
        }

    }
}
