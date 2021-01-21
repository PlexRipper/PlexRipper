using System;
using System.Linq;
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

        private readonly IPlexMovieService _plexMovieService;

        private readonly IPlexMediaService _plexMediaService;

        public PlexMediaController(IMapper mapper, INotificationsService notificationsService, IPlexTvShowService plexTvShowService,
            IPlexMovieService plexMovieService, IPlexMediaService plexMediaService) : base(
            mapper, notificationsService)
        {
            _plexTvShowService = plexTvShowService;
            _plexMovieService = plexMovieService;
            _plexMediaService = plexMediaService;
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

        // GET api/<PlexMedia>/5
        [HttpGet("thumb")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
        [ProducesResponseType(StatusCodes.Status204NoContent, Type = typeof(FileContentResult))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> GetThumb(int plexMediaId, PlexMediaType plexMediaType, int width, int height)
        {
            if (plexMediaId == 0)
            {
                return BadRequestInvalidId();
            }

            var result = await _plexMediaService.GetThumbnailImage(plexMediaId, plexMediaType, width, height);

            if (result.IsSuccess)
            {
                if (result.Value.Any())
                {
                    return File(result.Value, "image/jpeg");
                }

                return NoContent();
            }

            return InternalServerError(result);
        }

        // GET api/<PlexMedia>/5
        [HttpGet("banner")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
        [ProducesResponseType(StatusCodes.Status204NoContent, Type = typeof(FileContentResult))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> GetBanner(int plexMediaId, PlexMediaType plexMediaType, int width, int height)
        {
            if (plexMediaId == 0)
            {
                return BadRequestInvalidId();
            }

            var result = await _plexMediaService.GetBannerImage(plexMediaId, plexMediaType, width, height);

            if (result.IsSuccess)
            {
                if (result.Value.Any())
                {
                    return File(result.Value, "image/jpeg");
                }

                return NoContent();
            }

            return InternalServerError(result);
        }
    }
}