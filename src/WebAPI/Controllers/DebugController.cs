using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebugController : BaseController
    {
        private readonly IDownloadProgressScheduler _downloadProgressScheduler;

        public DebugController(IDownloadProgressScheduler downloadProgressScheduler, IMapper mapper,
            INotificationsService notificationsService) : base(mapper,
            notificationsService)
        {
            _downloadProgressScheduler = downloadProgressScheduler;
        }

        // GET: api/<DebugController>/StartServerDownloadProgress
        [HttpGet("StartServerDownloadProgress")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> StartDownloadProgressJob([FromQuery] int plexServerId)
        {
            var result = await _downloadProgressScheduler.StartDownloadProgressJob(plexServerId);
            return ToActionResult(result);
        }

        // GET: api/<DebugController>/StopServerDownloadProgress
        [HttpGet("StopServerDownloadProgress")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> StopDownloadProgressJob([FromQuery] int plexServerId)
        {
            var result = await _downloadProgressScheduler.StopDownloadProgressJob(plexServerId);
            return ToActionResult(result);
        }
    }
}