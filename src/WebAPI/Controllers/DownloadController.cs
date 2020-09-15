using System;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.WebAPI.Common.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRipper.Application.Common;
using PlexRipper.Domain;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadController : BaseController
    {
        private readonly IPlexDownloadService _plexDownloadService;
        private readonly IMapper _mapper;

        public DownloadController(IPlexDownloadService plexDownloadService, IMapper mapper) : base(mapper)
        {
            _plexDownloadService = plexDownloadService;
            _mapper = mapper;
        }

        // GET: api/<DownloadController>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<List<PlexServerDTO>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> Get()
        {
            var result = await _plexDownloadService.GetAllDownloadsAsync();
            if (result.IsFailed)
            {
                return InternalServerError(result);
            }

            var mapResult = _mapper.Map<List<PlexServerDTO>>(result.Value);
            return Ok(Result.Ok(mapResult));
        }


        /// <summary>
        /// GET: "api/(DownloadController)".
        /// </summary>
        /// <returns>Is successful.</returns>
        [HttpGet("clearcomplete")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<bool>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> ClearComplete()
        {
            var result = await _plexDownloadService.ClearCompleted();
            return result.IsFailed ? InternalServerError(result) : Ok(result);
        }

        // Post api/<DownloadController>/download/
        [HttpPost("download")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> DownloadMedia([FromBody] DownloadMediaDTO downloadMedia)
        {
            int plexMediaId = downloadMedia.PlexMediaId;
            int plexAccountId = downloadMedia.PlexAccountId;
            PlexMediaType type = downloadMedia.Type;
            if (plexMediaId <= 0)
            {
                return BadRequest(plexMediaId, nameof(plexMediaId));
            }

            if (plexAccountId <= 0)
            {
                return BadRequest(plexAccountId, nameof(plexAccountId));
            }

            var result = await _plexDownloadService.DownloadMediaAsync(plexAccountId, plexMediaId, type);
            if (result.Has400BadRequestError())
            {
                return BadRequest(result.ToResult());
            }

            return Ok(result);
        }

        // GET api/<DownloadController>/stop/{id:int}
        [HttpGet("stop/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> Stop(int id)
        {
            if (id <= 0)
            {
                return BadRequest(id, "Download Task Id");
            }

            var result = await _plexDownloadService.StopDownloadTask(id);
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
        public IActionResult Pause(int id)
        {
            if (id <= 0)
            {
                return BadRequest(id, "Download Task Id");
            }

            var result = _plexDownloadService.PauseDownloadTask(id);
            return result.IsFailed ? BadRequest(result) : Ok(result);
        }


        // DELETE api/<DownloadController>/5
        [HttpDelete("{downloadTaskId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> Delete(int downloadTaskId)
        {
            if (downloadTaskId <= 0)
            {
                return BadRequest(downloadTaskId, nameof(downloadTaskId));
            }

            var result = await _plexDownloadService.DeleteDownloadsAsync(downloadTaskId);
            return result.IsFailed ? BadRequest(result) : Ok(result);
        }
    }
}