using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
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

        public PlexServerController(IPlexServerService plexServerService, IMapper mapper, INotificationsService notificationsService) : base(mapper,
            notificationsService)
        {
            _plexServerService = plexServerService;
        }

        // GET api/<PlexServerController>/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<List<PlexServerDTO>>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
        public async Task<IActionResult> GetAll()
        {
            return ToActionResult<List<PlexServer>, List<PlexServerDTO>>(await _plexServerService.GetAllPlexServersAsync(false));
        }

        // GET api/<PlexServerController>/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexServerDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequestInvalidId();
            }

            return ToActionResult<PlexServer, PlexServerDTO>(await _plexServerService.GetServerAsync(id));
        }

        // GET api/<PlexServerController>/5/check
        [HttpGet("{id:int}/check")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexServerStatusDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
        public async Task<IActionResult> CheckStatus(int id, [FromQuery] int plexAccountId = 0)
        {
            if (id <= 0)
            {
                return BadRequestInvalidId();
            }

            return ToActionResult<PlexServerStatus, PlexServerStatusDTO>(await _plexServerService.CheckPlexServerStatusAsync(id, plexAccountId));
        }

        // GET api/<PlexServerController>/5/check
        [HttpPost("inspect")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
        public async Task<IActionResult> CheckStatus([FromBody] InspectServerDTO inspectServer)
        {
            if (!ModelState.IsValid) return ToResult(ModelState);

            return ToActionResult(
                await _plexServerService.InspectPlexServers(inspectServer.PlexAccountId, inspectServer.PlexServerIds));
        }
    }
}