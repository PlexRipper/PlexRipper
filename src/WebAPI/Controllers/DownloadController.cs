using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.DTO.WebApi;
using PlexRipper.Domain;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadController : BaseController
    {
        private readonly IPlexDownloadService _plexDownloadService;

        private readonly IMapper _mapper;

        private readonly INotificationsService _notificationsService;

        public DownloadController(IPlexDownloadService plexDownloadService, IMapper mapper, INotificationsService notificationsService) : base(mapper,
            notificationsService)
        {
            _plexDownloadService = plexDownloadService;
            _mapper = mapper;
            _notificationsService = notificationsService;
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

            var mapResult = _mapper.Map<List<DownloadTaskDTO>>(result.Value);
            return Ok(Result.Ok(mapResult));
        }

        // GET: api/<DownloadController>/inserver
        [HttpGet("inserver")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<List<PlexServerDTO>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> GetDownloadTasksInServer()
        {
            var result = await _plexDownloadService.GetDownloadTasksInServerAsync();
            if (result.IsFailed)
            {
                return InternalServerError(result);
            }

            var mapResult = _mapper.Map<List<PlexServerDTO>>(result.Value);
            return Ok(Result.Ok(mapResult));
        }

        /// <summary>
        /// Post: "api/(DownloadController)/clear".
        /// </summary>
        /// <returns>Is successful.</returns>
        [HttpPost("clear")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<bool>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> ClearComplete([FromBody] List<int> downloadTaskIds)
        {
            var result = await _plexDownloadService.ClearCompleted(downloadTaskIds);
            return result.IsFailed ? InternalServerError(result) : Ok(result);
        }

        /// <summary>
        /// POST: api/(DownloadController)/download/
        /// </summary>
        /// <param name="downloadMedia"></param>
        /// <returns></returns>
        [HttpPost("download")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> DownloadMedia([FromBody] List<DownloadMediaDTO> downloadMedias)
        {
            var result = await _plexDownloadService.DownloadMediaAsync(downloadMedias);

            if (result.IsFailed)
            {
                await _notificationsService.SendResult(result);
                return InternalServerError(result);
            }

            if (result.Has400BadRequestError())
            {
                return BadRequest(result.ToResult());
            }

            return Ok(result);
        }

        /// <summary>
        /// POST: api/(DownloadController)/stop/
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> Stop([FromBody] List<int> downloadTaskIds)
        {
            var result = await _plexDownloadService.StopDownloadTask(downloadTaskIds);
            return result.IsFailed ? BadRequest(result) : Ok(result);
        }

        // GET api/<DownloadController>/start/{id:int}
        [HttpGet("start/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> Start(int id)
        {
            if (id <= 0)
            {
                return BadRequest(id, "Download Task Id");
            }

            var result = await _plexDownloadService.StartDownloadTask(id);
            return result.IsFailed ? BadRequest(result) : Ok(result);
        }

        // GET api/<DownloadController>/restart/{id:int}
        [HttpGet("restart/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> Restart(int id)
        {
            if (id <= 0)
            {
                return BadRequest(id, "Download Task Id");
            }

            var result = await _plexDownloadService.RestartDownloadTask(id);
            return result.IsFailed ? BadRequest(result) : Ok(result);
        }

        // GET api/<DownloadController>/restart/{id:int}
        [HttpGet("pause/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> Pause(int id)
        {
            if (id <= 0)
            {
                return BadRequest(id, "Download Task Id");
            }

            var result = await _plexDownloadService.PauseDownloadTask(id);
            return result.IsFailed ? BadRequest(result) : Ok(result);
        }

        /// <summary>
        /// DELETE api/(DownloadController)/delete
        /// </summary>
        /// <param name="downloadTaskIds">The list of downloadTasks to delete by id.</param>
        /// <returns>HTTP Response.</returns>
        [HttpPost("delete")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> Delete([FromBody] List<int> downloadTaskIds)
        {
            if (downloadTaskIds.Count <= 0)
            {
                return BadRequest(Result.Fail("No list of download task Id's was given in the request body"));
            }

            Log.Debug($"Deleting the following DownloadTasks: {downloadTaskIds}");

            var result = await _plexDownloadService.DeleteDownloadTasksAsync(downloadTaskIds);
            if (result.Has400BadRequestError())
            {
                return BadRequest(result);
            }

            return result.IsFailed ? InternalServerError(result) : Ok(result);
        }
    }
}