using System.Collections.Generic;
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
    public class PlexServerController : BaseController
    {
        private readonly IPlexServerService _plexServerService;

        private readonly IMapper _mapper;

        public PlexServerController(IPlexServerService plexServerService, IMapper mapper, INotificationsService notificationsService) : base(mapper,
            notificationsService)
        {
            _mapper = mapper;
            _plexServerService = plexServerService;
        }

        // GET api/<PlexServerController>/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<List<PlexServerDTO>>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
        public async Task<IActionResult> GetAll()
        {
            var result = await _plexServerService.GetServersAsync();
            if (result.IsFailed)
            {
                if (result.Has400BadRequestError())
                {
                    return BadRequest(result.LogError());
                }
            }

            var mapResult = _mapper.Map<List<PlexServerDTO>>(result.Value);
            return Ok(Result.Ok(mapResult));
        }

        // GET api/<PlexServerController>/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexServerDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _plexServerService.GetServerAsync(id);
            if (result.IsFailed)
            {
                if (result.Has400BadRequestError())
                {
                    return BadRequest(result.LogError());
                }

                if (result.Has404NotFoundError())
                {
                    return NotFound(result.LogWarning());
                }
            }

            var mapResult = _mapper.Map<PlexServerDTO>(result.Value);
            return Ok(Result.Ok(mapResult));
        }

        // GET api/<PlexServerController>/byAccount/
        [HttpGet("byAccount/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<List<PlexServerDTO>>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
        public async Task<IActionResult> GetByAccountId(int id)
        {
            var result = await _plexServerService.GetServerAsync(id);
            if (result.IsFailed)
            {
                if (result.Has400BadRequestError())
                {
                    return BadRequest(result.LogError());
                }

                if (result.Has404NotFoundError())
                {
                    return NotFound(result.LogWarning());
                }
            }

            var mapResult = _mapper.Map<PlexServerDTO>(result.Value);
            return Ok(Result.Ok(mapResult));
        }

        // GET api/<PlexServerController>/5/check
        [HttpGet("{id:int}/check")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexServerStatusDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
        public async Task<IActionResult> CheckStatus(int id, [FromQuery] int plexAccountId = 0)
        {
            var result = await _plexServerService.CheckPlexServerStatusAsync(id, plexAccountId);
            if (result.IsFailed)
            {
                if (result.Has400BadRequestError())
                {
                    return BadRequest(result.LogError());
                }

                if (result.Has404NotFoundError())
                {
                    return NotFound(result.LogWarning());
                }
            }

            var mapResult = _mapper.Map<PlexServerStatusDTO>(result.Value);
            return Ok(Result.Ok(mapResult));
        }
    }
}