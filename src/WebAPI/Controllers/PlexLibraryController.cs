using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common;
using PlexRipper.Domain;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlexLibraryController : BaseController
    {
        private readonly IPlexLibraryService _plexLibraryService;

        public PlexLibraryController(IPlexLibraryService plexLibraryService, IMapper mapper, INotificationsService notificationsService) : base(
            mapper, notificationsService)
        {
            _plexLibraryService = plexLibraryService;
        }

        // GET api/<PlexLibrary>/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexLibraryDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> GetPlexLibrary(int id, [FromQuery] int plexAccountId = 0, bool allMedia = false)
        {
            if (id <= 0)
            {
                return BadRequest(id, nameof(id));
            }

            try
            {
                Log.Debug($"API Request: GetPlexLibrary(plexLibraryId = {id}, plexAccountId = {plexAccountId}, allMedia = {allMedia})");
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var data = await _plexLibraryService.GetPlexLibraryAsync(id, plexAccountId, !allMedia);

                if (data.IsFailed)
                {
                    return InternalServerError(data);
                }

                if (data.Value != null)
                {
                    var result = _mapper.Map<PlexLibraryDTO>(data.Value);
                    stopWatch.Stop();
                    Log.Debug(
                        $"Found {data.Value.MediaCount} in library {data.Value.Title} of type {data.Value.Type} - in {stopWatch.ElapsedMilliseconds}ms");
                    return Ok(Result.Ok(result));
                }

                stopWatch.Stop();
                string message = $"Could not find a {nameof(PlexLibrary)} with Id: {id}";
                Log.Warning(message);
                return NotFound(message);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        // GET api/<PlexLibrary>/5
        [HttpGet("inserver/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexServerDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> GetPlexLibraryInServer(int id, int plexAccountId = 0)
        {
            if (id <= 0)
            {
                return BadRequest(id, nameof(id));
            }

            try
            {
                var data = await _plexLibraryService.GetPlexLibraryInServerAsync(id, plexAccountId, true);

                if (data.IsFailed)
                {
                    return InternalServerError(data);
                }

                if (data.Value != null)
                {
                    var result = _mapper.Map<PlexServerDTO>(data.Value);
                    return Ok(Result.Ok(result));
                }

                string message = $"Could not find a {nameof(PlexLibrary)} with Id: {id}";
                Log.Warning(message);
                return NotFound(message);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        // POST api/<PlexLibrary>/refresh
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> RefreshLibrary([FromBody] RefreshPlexLibraryDTO refreshPlexLibraryDto)
        {
            var data = await _plexLibraryService.RefreshLibraryMediaAsync(refreshPlexLibraryDto.PlexLibraryId, refreshPlexLibraryDto.PlexAccountId);

            if (data.IsSuccess)
            {
                return Ok(Result.Ok());
            }

            string msg = $"Could not refresh {nameof(PlexLibrary)} with Id: {refreshPlexLibraryDto.PlexLibraryId}";
            Log.Warning(msg);
            return InternalServerError(data);
        }


    }
}