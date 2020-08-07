using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using PlexRipper.WebAPI.Common.DTO;
using System;
using System.Threading.Tasks;
using FluentResults;
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
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexServerDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> Get(int id)
        {
            if (id <= 0)
            {
                return BadRequestInvalidId();
            }
            try
            {
                var data = await _plexServerService.GetServerAsync(id);
                if (data.IsFailed)
                {
                    string message = $"Could not find a {nameof(PlexServer)} with Id: {id}";
                    Log.Warning(message);
                    return NotFound(message);
                }
                return Ok(_mapper.Map<PlexServerDTO>(data));
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }
    }
}