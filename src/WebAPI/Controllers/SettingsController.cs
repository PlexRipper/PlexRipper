using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain;
using PlexRipper.WebAPI.Common.DTO;
using System;
using System.Threading.Tasks;

namespace PlexRipper.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : BaseController
    {
        private readonly ISettingsService _settingsService;
        private readonly IMapper _mapper;

        public SettingsController(ISettingsService settingsService, IMapper mapper)
        {
            _settingsService = settingsService;
            _mapper = mapper;
        }

        // GET api/<SettingsController>/activeaccount/
        [HttpGet("activeaccount/")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlexAccountDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            try
            {
                var result = await _settingsService.GetActivePlexAccountAsync();
                if (result.IsFailed)
                {
                    return InternalServerError(result.Errors);
                }

                return Ok(_mapper.Map<PlexAccountDTO>(result.Value));

            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        // PUT api/<SettingsController>/activeaccount/5
        [HttpPut("activeaccount/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlexAccountDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Put(int id)
        {
            if (id <= 0) { return BadRequestInvalidId; }

            try
            {
                Log.Debug($"Setting the active plex account to {id}");

                var result = await _settingsService.SetActivePlexAccountAsync(id);
                if (result.IsFailed)
                {
                    return BadRequest(result.Errors);
                }

                return Ok(_mapper.Map<PlexAccountDTO>(result.Value));

            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }
    }
}
