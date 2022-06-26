using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application;
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

        // GET api/<PlexLibrary>/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<List<PlexLibraryDTO>>))]
        public async Task<IActionResult> GetPlexLibraries()
        {
            return ToActionResult<List<PlexLibrary>, List<PlexLibraryDTO>>(await _plexLibraryService.GetAllPlexLibrariesAsync());
        }

        // GET api/<PlexLibrary>/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexLibraryDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> GetPlexLibrary(int id, [FromQuery] bool allMedia = false)
        {
            if (id <= 0)
            {
                return BadRequest(id, nameof(id));
            }

            return ToActionResult<PlexLibrary, PlexLibraryDTO>(await _plexLibraryService.GetPlexLibraryAsync(id, !allMedia));
        }

        // GET api/<PlexLibrary>/5
        [HttpGet("inserver/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexServerDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> GetPlexLibraryInServer(int id)
        {
            if (id <= 0)
            {
                return BadRequest(id, nameof(id));
            }

            return ToActionResult<PlexServer, PlexServerDTO>(await _plexLibraryService.GetPlexLibraryInServerAsync(id, true));
        }

        // POST api/<PlexLibrary>/refresh
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexLibraryDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> RefreshLibrary([FromBody] RefreshPlexLibraryDTO refreshPlexLibraryDto)
        {
            if (refreshPlexLibraryDto is null)
            {
                return BadRequest();
            }

            return ToActionResult<PlexLibrary, PlexLibraryDTO>(await _plexLibraryService.RefreshLibraryMediaAsync(refreshPlexLibraryDto.PlexLibraryId));
        }

        // POST api/<PlexLibrary>/settings/default/destination/{id:int}
        [HttpPut("settings/default/destination")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> UpdateDefaultDestination([FromBody] UpdateDefaultDestinationDTO payload)
        {
            if (payload is null)
            {
                return BadRequest();
            }

            return ToActionResult(await _plexLibraryService.UpdateDefaultDestinationLibrary(payload.LibraryId, payload.FolderPathId));
        }
    }
}