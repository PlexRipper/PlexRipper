using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.FileSystem;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Types.FileSystem;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
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

        public FolderPathController(IFolderPathService folderPathService, IFileSystem fileSystem, IMapper mapper) : base(mapper)
        {
            _folderPathService = folderPathService;
            _fileSystem = fileSystem;
        }

        // GET: api/<FolderPathController>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<IEnumerable<FolderPath>>))]
        public async Task<IActionResult> Get()
        {
            var result = await _folderPathService.GetAllFolderPathsAsync();
            return Ok(result);
        }

        // GET: api/<FolderPathController>/directory?path=
        [HttpGet("directory")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<FileSystemResult>))]
        public IActionResult Get(string path)
        {
            var result = Result.Ok(_fileSystem.LookupContents(path, false, true));
            return Ok(result);
        }

        // POST: api/<FolderPathController>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<IEnumerable<FolderPath>>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
        public async Task<IActionResult> Put([FromBody] FolderPath folderPath)
        {
            var result = await _folderPathService.UpdateFolderPathAsync(folderPath);
            return result.IsFailed ? BadRequest(result) : Ok(result);
        }
    }
}
