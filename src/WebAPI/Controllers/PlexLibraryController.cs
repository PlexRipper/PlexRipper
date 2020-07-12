using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using PlexRipper.WebAPI.Common.DTO;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PlexRipper.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlexLibraryController : BaseController
    {

        private readonly IPlexLibraryService _plexLibraryService;
        private readonly IPlexAccountService _plexAccountService;
        private readonly IMapper _mapper;


        public PlexLibraryController(IPlexLibraryService plexLibraryService, IPlexAccountService plexAccountService, IMapper mapper)
        {

            _plexLibraryService = plexLibraryService;
            _plexAccountService = plexAccountService;
            _mapper = mapper;
        }

        // GET api/<PlexLibrary>/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id, int plexAccountId)
        {
            if (id <= 0) { return BadRequestInvalidId; }

            var data = await _plexLibraryService.GetPlexLibraryAsync(id, plexAccountId);

            if (data.IsFailed)
            {
                return BadRequest(data.Errors);
            }

            if (data.Value != null)
            {
                var result = _mapper.Map<PlexLibraryDTO>(data.Value);
                Log.Debug($"Found {data.Value.GetMediaCount} in library {data.Value.Title} of type {data.Value.Type}");
                return Ok(result);

            }
            string message = $"Could not find a {nameof(PlexLibrary)} with Id: {id}";
            Log.Warning(message);
            return NotFound(message);
        }

        // POST api/<PlexLibrary>/
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlexLibraryDTO))]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(List<Error>))]
        public async Task<IActionResult> RefreshLibrary([FromBody] RefreshPlexLibraryDTO refreshPlexLibraryDto)
        {
            var data = await _plexLibraryService.RefreshLibraryMediaAsync(refreshPlexLibraryDto.PlexAccountId, refreshPlexLibraryDto.PlexLibraryId);

            if (data.IsFailed)
            {
                return InternalServerError(data.Errors);
            }

            var mapResult = _mapper.Map<PlexLibraryDTO>(data.Value);
            return Ok(mapResult);
        }
    }
}
