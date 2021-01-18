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
    public class PlexMediaController : BaseController
    {
        private readonly IPlexTvShowService _plexTvShowService;

        private readonly IMapper _mapper;

        public PlexMediaController(IPlexTvShowService plexTvShowService, IMapper mapper, INotificationsService notificationsService) : base(
            mapper, notificationsService)
        {
            _plexTvShowService = plexTvShowService;
            _mapper = mapper;
        }

        // GET api/<PlexMedia>/tvshow/5
        [HttpGet("tvshow/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexTvShowDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> GetTvShow(int id)
        {
            if (id <= 0)
            {
                return BadRequest(id, nameof(id));
            }

            try
            {
                var data = await _plexTvShowService.GetTvShow(id);

                if (data.IsFailed)
                {
                    return InternalServerError(data);
                }

                if (data.Value != null)
                {
                    return Ok(Result.Ok(_mapper.Map<PlexTvShowDTO>(data.Value)));
                }

                string message = $"Could not find a {nameof(PlexTvShow)} with Id: {id}";
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