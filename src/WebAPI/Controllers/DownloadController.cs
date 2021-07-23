using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.WebApi;
using PlexRipper.Domain;
using PlexRipper.WebAPI.Common;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.FluentResult;

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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<List<DownloadTaskDTO>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> GetDownloadTasks()
        {
            var result = await _plexDownloadService.GetDownloadTasksAsync();
            if (result.IsFailed)
            {
                return InternalServerError(result);
            }

            return ToActionResult<List<DownloadTaskDTO>, List<DownloadTaskDTO>>(ControllerHelpers.ConvertToDownloadTaskDTOHierarchy(result.Value, _mapper));
        }

        /// <summary>
        /// POST: "api/(DownloadController)/clear".
        /// </summary>
        /// <returns>Is successful.</returns>
        [HttpPost("clear")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> ClearComplete([FromBody] List<int> downloadTaskIds)
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

        // POST api/<DownloadController>/start/{id:int}
        [HttpPost("start")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> Start([FromBody] List<int> downloadTaskIds)
        {
            if (!downloadTaskIds.Any())
            {
                return BadRequest(Result.Fail("No list of download task Id's was given in the request body"));
            }

            return ToActionResult(await _plexDownloadService.StartDownloadTask(downloadTaskIds));
        }

        // POST api/<DownloadController>/restart/{id:int}
        [HttpPost("pause")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> Pause([FromBody] List<int> downloadTaskIds)
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
        public async Task<IActionResult> Delete([FromBody] List<int> downloadTaskIds)
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
        public async Task<IActionResult> Stop([FromBody] List<int> downloadTaskIds)
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