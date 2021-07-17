using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common;
using PlexRipper.Domain;
using PlexRipper.WebAPI.Common.DTO.FolderPath;
using PlexRipper.WebAPI.Common.FluentResult;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PlexRipper.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FolderPathController : BaseController
    {
        private readonly IFolderPathService _folderPathService;

        private readonly IFileSystem _fileSystem;

        public FolderPathController(IFolderPathService folderPathService, IFileSystem fileSystem, IMapper mapper,
            INotificationsService notificationsService) : base(mapper, notificationsService)
        {
            _folderPathService = folderPathService;
            _fileSystem = fileSystem;
        }

        // GET: api/<FolderPathController>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<List<FolderPathDTO>>))]
        public async Task<IActionResult> Get()
        {
            var result = await _folderPathService.GetAllFolderPathsAsync();
            var mapResult = _mapper.Map<List<FolderPathDTO>>(result.Value);
            return Ok(Result.Ok(mapResult));
        }

        // GET: api/<FolderPathController>/directory?path=
        [HttpGet("directory")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<FileSystemDTO>))]
        public IActionResult Get(string path)
        {
            path = path == "null" ? string.Empty : path;
            var mapResult = _mapper.Map<FileSystemDTO>(_fileSystem.LookupContents(path, false, true));
            return Ok(Result.Ok(mapResult));
        }

        // PUT: api/<FolderPathController>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<List<FolderPathDTO>>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> Put([FromBody] FolderPathDTO folderPathDto)
        {
            var folderPath = _mapper.Map<FolderPath>(folderPathDto);
            var result = await _folderPathService.UpdateFolderPathAsync(folderPath);
            return result.IsFailed ? BadRequest(result) : Ok(Result.Ok(_mapper.Map<FolderPathDTO>(result.Value)));
        }

        // POST: api/<FolderPathController>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<FolderPathDTO>))]
        public async Task<IActionResult> Create([FromBody] FolderPathDTO folderPathDto)
        {
            var folderPath = _mapper.Map<FolderPath>(folderPathDto);
            var result = await _folderPathService.CreateFolderPath(folderPath);
            var mapResult = _mapper.Map<FolderPathDTO>(result.Value);
            return Ok(Result.Ok(mapResult));
        }

        // Delete: api/<FolderPathController>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequestInvalidId();
            }

            var result = await _folderPathService.DeleteFolderPathAsync(id);
            return Ok(result);
        }
    }
}