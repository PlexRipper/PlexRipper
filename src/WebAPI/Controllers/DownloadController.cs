using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application;
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

        // GET api/<DownloadController>/start/{id:int}
        [HttpGet("start/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> StartCommand(int id)
        {
            return id <= 0 ? BadRequestInvalidId() : ToActionResult(await _plexDownloadService.StartDownloadTask(id));
        }

        // GET api/<DownloadController>/pause/{id:int}
        [HttpGet("pause/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> PauseCommand(int id)
        {
            return id <= 0 ? BadRequestInvalidId() : ToActionResult(await _plexDownloadService.PauseDownloadTask(id));
        }

        // GET api/<DownloadController>/restart/{id:int}
        [HttpGet("restart/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> RestartCommand(int id)
        {
            return id <= 0 ? BadRequestInvalidId() : ToActionResult(await _plexDownloadService.RestartDownloadTask(id));
        }

        // GET: api/(DownloadController)/stop/{id:int}
        [HttpGet("stop/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> StopCommand(int id)
        {
            return id <= 0 ? BadRequestInvalidId() : ToActionResult(await _plexDownloadService.StopDownloadTask(id));
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

            var result = await _plexDownloadService.DeleteDownloadTasksAsync(downloadTaskIds);
            return ToActionResult(result.ToResult());
        }

        // GET: api/(DownloadController)/detail/{id:int}
        [HttpGet("detail/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> GetDetail(int id, CancellationToken token)
        {
            if (id <= 0)
            {
                return BadRequestInvalidId();
            }

            var downloadTaskResult = await _plexDownloadService.GetDownloadTaskDetailAsync(id, token);
            return ToActionResult<DownloadTask, DownloadTaskDTO>(downloadTaskResult);
        }

        #endregion
    }
}