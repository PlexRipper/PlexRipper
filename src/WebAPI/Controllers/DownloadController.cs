using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.WebAPI.Common.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadController : ControllerBase
    {
        private readonly IPlexDownloadService _plexDownloadService;
        private readonly IMapper _mapper;

        public DownloadController(IPlexDownloadService plexDownloadService, IMapper mapper)
        {
            _plexDownloadService = plexDownloadService;
            _mapper = mapper;
        }

        // GET: api/<DownloadController>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<DownloadTaskDTO>))]
        public async Task<IActionResult> Get()
        {
            var result = await _plexDownloadService.GetAllDownloadsAsync();
            if (result.IsFailed)
            {
                return BadRequest(result.Errors);
            }
            var mapResult = _mapper.Map<List<DownloadTaskDTO>>(result.Value);
            return Ok(mapResult);
        }

        // Get api/<DownloadController>/movie/{plexMovieId:int}?plexAccountId={id}
        [HttpGet("movie/{plexMovieId:int}")]
        public async Task<IActionResult> Get(int plexMovieId, int plexAccountId)
        {
            var result = await _plexDownloadService.DownloadMovieAsync(plexAccountId, plexMovieId);
            if (result.IsFailed)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Value);
        }

        // POST api/<DownloadController>/movie/
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<DownloadController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<DownloadController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
