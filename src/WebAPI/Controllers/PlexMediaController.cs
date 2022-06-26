using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application;
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexMediaDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
        public async Task<IActionResult> GetTvShow(int id)
        {
            if (id <= 0)
            {
                return BadRequest(id, nameof(id));
            }

            var data = await _plexTvShowService.GetTvShow(id);
            return ToActionResult<PlexTvShow, PlexMediaDTO>(data);
        }

        // GET api/<PlexMedia>/5
        [HttpGet("thumb")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
        [ProducesResponseType(StatusCodes.Status204NoContent, Type = typeof(FileContentResult))]
        [ProducesResponseType(StatusCodes.Status408RequestTimeout, Type = typeof(ResultDTO))]
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

            return ToActionResult(result.ToResult());
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

            return ToActionResult(result.ToResult());
        }
    }
}