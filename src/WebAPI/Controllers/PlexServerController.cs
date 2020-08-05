using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using PlexRipper.WebAPI.Common.DTO;
using System;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PlexRipper.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlexServerController : BaseController
    {

        private readonly IPlexServerService _plexServerService;
        private readonly IMapper _mapper;

        public PlexServerController(IPlexServerService plexServerService, IMapper mapper) : base(mapper)
        {
            _mapper = mapper;
            _plexServerService = plexServerService;
        }

        // GET api/<PlexServerController>/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(int id)
        {
            if (id <= 0) { return BadRequestInvalidId; }

            try
            {
                var data = await _plexServerService.GetServerAsync(id);
                if (data != null)
                {
                    return Ok(_mapper.Map<PlexServerDTO>(data));
                }
                string message = $"Could not find a {nameof(PlexServer)} with Id: {id}";
                Log.Warning(message);
                return NotFound(message);

            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

    }
}
