using Application.Contracts;
using AutoMapper;
using FileSystem.Contracts;
using Logging.Interface;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application;
using PlexRipper.WebAPI.Common.DTO.FolderPath;

namespace PlexRipper.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FolderPathController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IFileSystem _fileSystem;

    public FolderPathController(
        ILog log,
        IMediator mediator,
        IFileSystem fileSystem,
        IMapper mapper,
        INotificationsService notificationsService) : base(log, mapper, notificationsService)
    {
        _mediator = mediator;
        _fileSystem = fileSystem;
    }

    // GET: api/<FolderPathController>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<List<FolderPathDTO>>))]
    public async Task<IActionResult> Get()
    {
        var result = await _mediator.Send(new GetAllFolderPathsQuery());
        return ToActionResult<List<FolderPath>, List<FolderPathDTO>>(result);
    }

    // GET: api/<FolderPathController>/directory?path=
    [HttpGet("directory")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<FileSystemDTO>))]
    public IActionResult Get(string path)
    {
        path = path == "null" ? string.Empty : path;
        return ToActionResult<FileSystemResult, FileSystemDTO>(_fileSystem.LookupContents(path, false, true));
    }

    // PUT: api/<FolderPathController>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<List<FolderPathDTO>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    public async Task<IActionResult> Put([FromBody] FolderPathDTO folderPathDto)
    {
        var folderPath = _mapper.Map<FolderPath>(folderPathDto);
        var result = await _mediator.Send(new UpdateFolderPathCommand(folderPath));
        return ToActionResult<FolderPath, FolderPathDTO>(result);
    }

    // POST: api/<FolderPathController>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<FolderPathDTO>))]
    public async Task<IActionResult> Create([FromBody] FolderPathDTO folderPathDto)
    {
        var folderPath = _mapper.Map<FolderPath>(folderPathDto);
        var result = await _mediator.Send(new CreateFolderPathCommand(folderPath));
        return ToActionResult<FolderPath, FolderPathDTO>(result);
    }

    // Delete: api/<FolderPathController>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
            return BadRequestInvalidId();

        var result = await _mediator.Send(new DeleteFolderPathCommand(id));
        return ToActionResult(result);
    }
}