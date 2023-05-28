using Application.Contracts;
using AutoMapper;
using Data.Contracts;
using Logging.Interface;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.Extensions;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlexMediaController : BaseController
{
    private readonly IMediator _mediator;

    private readonly IPlexMovieService _plexMovieService;

    private readonly IPlexMediaService _plexMediaService;

    public PlexMediaController(
        ILog log,
        IMediator mediator,
        IMapper mapper,
        INotificationsService notificationsService,
        IPlexMovieService plexMovieService,
        IPlexMediaService plexMediaService) : base(log, mapper, notificationsService)
    {
        _mediator = mediator;
        _plexMovieService = plexMovieService;
        _plexMediaService = plexMediaService;
    }

    /// <summary>
    /// Get the <see cref="PlexTvShow"/> with the <see cref="PlexTvShowSeason"/> and <see cref="PlexTvShowEpisode"/> in a minimal format.
    /// </summary>
    /// <param name="id">The id of the <see cref="PlexTvShow"/></param>
    /// <returns></returns>
    [HttpGet("tvshow/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexMediaSlimDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> GetTvShow(int id)
    {
        if (id <= 0)
            return BadRequest(id, nameof(id));

        var result = await _mediator.Send(new GetPlexTvShowByIdWithEpisodesQuery(id));

        if (result.IsFailed)
            return ToActionResult(result.ToResult());

        var dto = _mapper.Map<PlexMediaSlimDTO>(result.Value).SetIndex();

        return Ok(Result.Ok(dto));
    }

    /// <summary>
    /// Get the <see cref="PlexTvShow"/> without the <see cref="PlexTvShowSeason"/> and <see cref="PlexTvShowEpisode"/>
    /// </summary>
    /// <param name="id">The id of the <see cref="PlexTvShow"/></param>
    /// <returns></returns>
    [HttpGet("tvshow/detail/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexMediaDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> GetTvShowDetail(int id)
    {
        if (id <= 0)
            return BadRequest(id, nameof(id));

        var result = await _mediator.Send(new GetPlexTvShowByIdQuery(id));

        return ToActionResult<PlexTvShow, PlexMediaDTO>(result);
    }

    // GET api/<PlexMedia>/library/5
    [HttpGet("library/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<List<PlexMediaSlimDTO>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> GetLibraryMedia(int id, [FromQuery] int page, [FromQuery] int size)
    {
        if (id <= 0)
            return BadRequest(id, nameof(id));

        var result = await _mediator.Send(new GetPlexMediaDataByLibraryIdQuery(id, page, size));
        if (result.IsFailed || !result.Value.Any())
            return ToActionResult(result.ToResult());

        var plexServerId = result.Value[0].PlexServerId;

        var plexServerConnection = await _mediator.Send(new GetPlexServerConnectionByPlexServerIdQuery(plexServerId));
        if (plexServerConnection.IsFailed)
            return ToActionResult(plexServerConnection.ToResult());

        var connection = plexServerConnection.Value;
        var plexServerToken = await _mediator.Send(new GetPlexServerTokenQuery(plexServerId));

        var dto = _mapper.Map<List<PlexMediaSlimDTO>>(result.Value).SetIndex();

        foreach (var mediaSlimDto in dto)
        {
            mediaSlimDto.ThumbUrl = connection.GetThumbUrl(mediaSlimDto.ThumbUrl);
            mediaSlimDto.ThumbUrl += $"&X-Plex-Token={plexServerToken.Value}";
        }

        return Ok(Result.Ok(dto));
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
            return BadRequestInvalidId();

        var result = await _plexMediaService.GetThumbnailImage(plexMediaId, plexMediaType, width, height);

        if (result.IsSuccess)
        {
            if (result.Value.Any())
                return File(result.Value, "image/jpeg");

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
            return BadRequestInvalidId();

        var result = await _plexMediaService.GetBannerImage(plexMediaId, plexMediaType, width, height);

        if (result.IsSuccess)
        {
            if (result.Value.Any())
                return File(result.Value, "image/jpeg");

            return NoContent();
        }

        return ToActionResult(result.ToResult());
    }
}