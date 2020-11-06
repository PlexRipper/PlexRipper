using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Domain;
using PlexRipper.WebAPI.Common.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Application.Common;
using PlexRipper.WebAPI.Common.FluentResult;

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
            var mapResult = _mapper.Map<List<PlexServerDTO>>(result);
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
            var mapResult = _mapper.Map<PlexServerDTO>(result);
            return Ok(Result.Ok(mapResult));
        }
    }
}