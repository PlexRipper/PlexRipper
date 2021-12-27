using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application;
using PlexRipper.Domain;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.FluentResult;
using PlexRipper.WebAPI.SignalR.Common;

namespace PlexRipper.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadController : BaseController
    {
        private readonly IPlexDownloadService _plexDownloadService;

        public DownloadController(IPlexDownloadService plexDownloadService, IMapper mapper, INotificationsService notificationsService) : base(mapper,
            notificationsService)
        {
            _plexDownloadService = plexDownloadService;
        }

        // GET: api/<DownloadController>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<List<ServerDownloadProgressDTO>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> GetDownloadTasks()
        {
            var result = await _plexDownloadService.GetDownloadTasksAsync();
            if (result.IsFailed)
            {
                return InternalServerError(result.ToResult());
            }

            return ToActionResult<List<DownloadTask>, List<ServerDownloadProgressDTO>>(result);
        }

        /// <summary>
        /// POST: "api/(DownloadController)/clear".
        /// </summary>
        /// <returns>Is successful.</returns>
        [HttpPost("clear")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> ClearCompleted([FromBody] List<int> downloadTaskIds)
        {
            return ToActionResult(await _plexDownloadService.ClearCompleted(downloadTaskIds));
        }

        /// <summary>
        /// POST: api/(DownloadController)/download/
        /// </summary>
        /// <param name="downloadMedias"></param>
        /// <returns></returns>
        [HttpPost("download")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> DownloadMedia([FromBody] List<DownloadMediaDTO> downloadMedias)
        {
            return ToActionResult(await _plexDownloadService.DownloadMediaAsync(downloadMedias));
        }

        #region BatchCommands

        // POST api/<DownloadController>/start/
        [HttpPost("start")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> StartCommand([FromBody] List<int> downloadTaskIds)
        {
            if (!downloadTaskIds.Any())
            {
                return BadRequest(Result.Fail("No list of download task Id's was given in the request body"));
            }

            return ToActionResult(await _plexDownloadService.StartDownloadTask(downloadTaskIds));
        }

        // POST api/<DownloadController>/pause/
        [HttpPost("pause")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> PauseCommand([FromBody] List<int> downloadTaskIds)
        {
            if (!downloadTaskIds.Any())
            {
                return BadRequest(Result.Fail("No list of download task Id's was given in the request body"));
            }

            return ToActionResult(await _plexDownloadService.PauseDownloadTask(downloadTaskIds));
        }

        /// <summary>
        /// HttpPost api/(DownloadController)/delete
        /// </summary>
        /// <param name="downloadTaskIds">The list of downloadTasks to delete by id.</param>
        /// <returns>HTTP Response.</returns>
        [HttpPost("delete")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> DeleteCommand([FromBody] List<int> downloadTaskIds)
        {
            if (!downloadTaskIds.Any())
            {
                return BadRequest(Result.Fail("No list of download task Id's was given in the request body"));
            }

            return ToActionResult(await _plexDownloadService.DeleteDownloadTasksAsync(downloadTaskIds));
        }

        // POST api/<DownloadController>/restart/{id:int}
        [HttpPost("restart")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> Restart([FromBody] List<int> downloadTaskIds)
        {
            if (!downloadTaskIds.Any())
            {
                return BadRequest(Result.Fail("No list of download task Id's was given in the request body"));
            }

            return ToActionResult(await _plexDownloadService.RestartDownloadTask(downloadTaskIds));
        }

        // POST: api/(DownloadController)/stop/
        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> StopCommand([FromBody] List<int> downloadTaskIds)
        {
            if (!downloadTaskIds.Any())
            {
                return BadRequest(Result.Fail("No list of download task Id's was given in the request body"));
            }

            return ToActionResult(await _plexDownloadService.StopDownloadTask(downloadTaskIds));
        }

        #endregion
    }
}