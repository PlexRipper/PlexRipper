using Carter;
using Carter.Request;
using Carter.Response;
using Microsoft.AspNetCore.Http;
using PlexRipper.Application.Common.Interfaces;
using System.Threading.Tasks;

namespace PlexRipper.WebAPI.Controllers
{
    public class PlexServerController : CarterModule
    {
        private readonly IPlexService _plexService;
        private readonly IAccountService _accountService;

        public PlexServerController(IPlexService plexService, IAccountService accountService) : base("/api")
        {
            _plexService = plexService;
            _accountService = accountService;
            string path = "/plex";

            Get(path + "/servers/{id:int}", GetServersByAccountId);
            Get(path + "/servers/{id:int}/libraries/", GetLibrariesByServerId);

        }

        private async Task GetServersByAccountId(HttpRequest req, HttpResponse res)
        {
            int accountId = req.RouteValues.As<int>("id");
            var data = _accountService.GetServers(accountId);
            await res.Negotiate(data);
        }

        private async Task GetLibrariesByServerId(HttpRequest req, HttpResponse res)
        {
            int plexServerId = req.RouteValues.As<int>("id");
            var data = await _plexService.GetLibrariesByPlexServerIdAsync(plexServerId);
            await res.Negotiate(data);
        }
    }
}
