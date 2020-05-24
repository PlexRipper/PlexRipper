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
        private readonly IPlexServerService _plexServerService;
        private readonly IAccountService _accountService;

        public PlexServerController(IPlexService plexService, IPlexServerService plexServerService, IAccountService accountService) : base("/api")
        {
            _plexService = plexService;
            _plexServerService = plexServerService;
            _accountService = accountService;
            string path = "/plex";

            Get(path + "/servers/{id:int}", GetServersByAccountIdAsync);
            // Get(path + "/servers/{id:int}/libraries/", GetLibrariesByServerId);

        }

        private Task GetServersByAccountIdAsync(HttpRequest req, HttpResponse res)
        {
            int accountId = req.RouteValues.As<int>("id");
            var data = _accountService.GetServersAsync(accountId);
            return res.Negotiate(data);
        }

        //private async Task GetLibrariesByServerId(HttpRequest req, HttpResponse res)
        //{
        //    int plexServerId = req.RouteValues.As<int>("id");
        //    var data = await _plexServerService. .etLibrariesByPlexServerIdAsync(plexServerId);
        //    await res.Negotiate(data);
        //}
    }
}
