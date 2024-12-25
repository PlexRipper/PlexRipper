using System.IO.Abstractions;
using Application.Contracts;
using FastEndpoints;
using FileSystem.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public class GetFolderPathDirectoryRequest
{
    public GetFolderPathDirectoryRequest(string path = "")
    {
        Path = path;
    }

    [QueryParam, BindFrom("path")]
    public string Path { get; init; }
}

public class GetFolderPathDirectoryRequestValidator : Validator<GetFolderPathDirectoryRequest>
{
    public GetFolderPathDirectoryRequestValidator()
    {
        RuleFor(x => x.Path).NotNull();
    }
}

public class GetFolderPathDirectoryEndpoint : BaseEndpoint<GetFolderPathDirectoryRequest, FileSystemDTO>
{
    private readonly ILog _log;
    private readonly IDirectory _directory;
    private readonly IPath _path;
    private readonly IDiskProvider _diskProvider;

    public override string EndpointPath => ApiRoutes.FolderPathController + "/directory";

    public GetFolderPathDirectoryEndpoint(ILog log, IDirectory directory, IPath path, IDiskProvider diskProvider)
    {
        _log = log;
        _directory = directory;
        _path = path;
        _diskProvider = diskProvider;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Summary(x =>
        {
            x.Summary = "Get all the FolderPaths entities in the database";
        });
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<FileSystemDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(GetFolderPathDirectoryRequest req, CancellationToken ct)
    {
        var path = req.Path!;

        var result = LookupContents(path, false, true);

        await SendFluentResult(result, x => x.ToDTO(), ct);
    }

    private Result<FileSystemResult> LookupContents(
        string query,
        bool includeFiles,
        bool allowFoldersWithoutTrailingSlashes
    )
    {
        _log.Debug("Looking up path: {Query}", query);

        var defaultResult = new FileSystemResult
        {
            Directories = _diskProvider
                .GetAllMounts()
                .Select(d => new FileSystemModel
                {
                    Type = FileSystemEntityType.Drive,
                    Name = _diskProvider.GetVolumeName(d),
                    Path = d.RootDirectory.FullName,
                    LastModified = d.RootDirectory.LastWriteTimeUtc,
                    Extension = string.Empty,
                    Size = d.TotalSize,
                    HasReadPermission = d.CanRead(),
                    HasWritePermission = d.CanWrite(),
                })
                .ToList(),
            Files = [],
            Parent = "",
            Current = null,
        };

        // If path is invalid return root file system
        if (string.IsNullOrWhiteSpace(query))
            return Result.Ok(defaultResult);

        var directoryExists = _directory.Exists(query);
        if (!directoryExists)
            return Result.Ok(defaultResult);

        if (allowFoldersWithoutTrailingSlashes)
            return GetFileSystemResults(query, includeFiles);

        var lastSeparatorIndex = query.LastIndexOf(_path.DirectorySeparatorChar);
        var path = query.Substring(0, lastSeparatorIndex + 1);

        if (lastSeparatorIndex != -1)
            return GetFileSystemResults(path, includeFiles);

        return Result.Ok(defaultResult);
    }

    private Result<FileSystemResult> GetFileSystemResults(string path, bool includeFiles)
    {
        try
        {
            var directoriesResult = _diskProvider.GetDirectories(path);
            if (directoriesResult.IsFailed)
                return directoriesResult.ToResult();

            if (includeFiles)
            {
                var filesResult = _diskProvider.GetFiles(path);
                if (filesResult.IsFailed)
                    return filesResult.ToResult();

                return Result.Ok(
                    new FileSystemResult()
                    {
                        Parent = _diskProvider.GetParent(path),
                        Directories = directoriesResult.Value,
                        Files = filesResult.Value,
                        Current = new DirectoryInfo(path).ToModel(),
                    }
                );
            }

            return Result.Ok(
                new FileSystemResult()
                {
                    Parent = _diskProvider.GetParent(path),
                    Directories = directoriesResult.Value,
                    Files = [],
                    Current = new DirectoryInfo(path).ToModel(),
                }
            );
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
