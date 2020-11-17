using System;
using System.Threading.Tasks;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common;
using PlexRipper.Application.Settings.Models;
using PlexRipper.Domain;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : BaseController
    {
        private readonly ISettingsService _settingsService;

        private readonly IMapper _mapper;

        public SettingsController(ISettingsService settingsService, IMapper mapper) : base(mapper)
        {
            _settingsService = settingsService;
            _mapper = mapper;
        }

        // GET api/<SettingsController>/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<SettingsModel>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public IActionResult GetSettings()
        {
            try
            {
                var result = _settingsService.GetSettings();
                if (result.IsFailed)
                {
                    return InternalServerError(result);
                }

                return Ok(Result.Ok(result.Value));
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        // GET api/<SettingsController>/activeaccount/
        [HttpGet("activeaccount/")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexAccountDTO>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> GetActiveAccount()
        {
            try
            {
                var result = await _settingsService.GetActivePlexAccountAsync();
                if (result.IsFailed)
                {
                    return InternalServerError(result);
                }

                var mapResult = _mapper.Map<PlexAccountDTO>(result.Value);
                return Ok(Result.Ok(mapResult));
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        // PUT api/<SettingsController>/
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> UpdateSettings([FromBody] SettingsModel settingsModel)
        {
            try
            {
                var result = await _settingsService.UpdateSettings(settingsModel);
                if (result.IsFailed)
                {
                    return BadRequest(result);
                }

                return Ok(Result.Ok(true));
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        // PUT api/<SettingsController>/activeaccount/5
        [HttpPut("activeaccount/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public IActionResult UpdateActiveAccount(int id)
        {
            if (id <= 0)
            {
                return BadRequestInvalidId();
            }

            try
            {
                Log.Debug($"Setting the active plex account to {id}");

                var result = _settingsService.SetActivePlexAccountAsync(id);
                if (result.IsFailed)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }
    }
}